using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Api.Endpoints.Catalog.Products;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.Products;

public sealed class ListProductsEndpointTests(
    ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private static readonly Guid EachUnitOfMeasureId =
        Guid.Parse(
            "10000000-0000-0000-0000-000000000001");

    private static readonly Guid StandardGstTaxCategoryId =
        Guid.Parse(
            "20000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task Get_WithDefaultParameters_ReturnsOrganisationProducts()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid otherOrganisationId = Guid.NewGuid();
        string suffix = CreateSuffix();

        Product productB =
            CreateProduct(
                organisationId,
                $"LIST-{suffix}-B",
                "Product B");

        Product productA =
            CreateProduct(
                organisationId,
                $"LIST-{suffix}-A",
                "Product A");

        Product otherProduct =
            CreateProduct(
                otherOrganisationId,
                $"LIST-{suffix}-OTHER",
                "Other Organisation Product");

        dbContext.Products.AddRange(
            productB,
            productA,
            otherProduct);

        await dbContext.SaveChangesAsync();

        using HttpClient client =
            CreateAuthenticatedClient(organisationId);

        try
        {
            HttpResponseMessage response =
                await client.GetAsync(
                    "/api/v1/catalog/products");

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            Assert.Equal(
                "application/json",
                response.Content.Headers
                    .ContentType?.MediaType);

            string responseJson =
                await response.Content.ReadAsStringAsync();

            using JsonDocument jsonDocument =
                JsonDocument.Parse(responseJson);

            JsonElement firstItemJson =
                jsonDocument.RootElement
                    .GetProperty("items")[0];

            JsonElement statusElement =
                firstItemJson.GetProperty("status");

            Assert.Equal(
                JsonValueKind.String,
                statusElement.ValueKind);

            Assert.Equal(
                "Active",
                statusElement.GetString());

            ListProductsResponse? responseBody =
                await response.Content
                    .ReadFromJsonAsync<ListProductsResponse>();

            Assert.NotNull(responseBody);

            Assert.Equal(1, responseBody.PageNumber);
            Assert.Equal(20, responseBody.PageSize);
            Assert.Equal(2, responseBody.TotalCount);
            Assert.Equal(1, responseBody.TotalPages);
            Assert.Equal(2, responseBody.Items.Count);

            Assert.Equal(
                productA.Id,
                responseBody.Items[0].ProductId);

            Assert.Equal(
                productB.Id,
                responseBody.Items[1].ProductId);

            Assert.DoesNotContain(
                responseBody.Items,
                item =>
                    item.ProductId == otherProduct.Id);

            ListProductItemResponse firstItem =
                responseBody.Items[0];

            Assert.Equal(productA.Sku, firstItem.Sku);
            Assert.Equal(productA.Name, firstItem.Name);

            Assert.Equal(
                productA.UnitOfMeasureId,
                firstItem.UnitOfMeasureId);

            Assert.Equal(
                productA.ProductCategoryId,
                firstItem.ProductCategoryId);

            Assert.Equal(
                productA.TaxCategoryId,
                firstItem.TaxCategoryId);

            Assert.Equal(
                ProductStatus.Active,
                firstItem.Status);

            Assert.Equal(
                productA.CreatedAt,
                firstItem.CreatedAt);

            Assert.Null(firstItem.LastModifiedAt);
        }
        finally
        {
            await DeleteProductsAsync(
                dbContext,
                organisationId,
                otherOrganisationId);
        }
    }

    [Fact]
    public async Task Get_WithSearchStatusAndPagination_ReturnsFilteredPage()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        string suffix = CreateSuffix();

        Product inactiveChairA =
            CreateProduct(
                organisationId,
                $"FILTER-{suffix}-A",
                "Office Chair");

        Product inactiveChairB =
            CreateProduct(
                organisationId,
                $"FILTER-{suffix}-B",
                "Dining Chair");

        Product activeChair =
            CreateProduct(
                organisationId,
                $"FILTER-{suffix}-C",
                "Meeting Chair");

        DateTimeOffset modifiedAt =
            DateTimeOffset.UtcNow.AddMinutes(-1);

        inactiveChairA.Deactivate(
            modifiedAt,
            Guid.NewGuid());

        inactiveChairB.Deactivate(
            modifiedAt,
            Guid.NewGuid());

        dbContext.Products.AddRange(
            inactiveChairA,
            inactiveChairB,
            activeChair);

        await dbContext.SaveChangesAsync();

        using HttpClient client =
            CreateAuthenticatedClient(organisationId);

        try
        {
            HttpResponseMessage response =
                await client.GetAsync(
                    "/api/v1/catalog/products"
                    + "?search=chair"
                    + "&status=Inactive"
                    + "&pageNumber=2"
                    + "&pageSize=1");

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            ListProductsResponse? responseBody =
                await response.Content
                    .ReadFromJsonAsync<ListProductsResponse>();

            Assert.NotNull(responseBody);

            Assert.Equal(2, responseBody.PageNumber);
            Assert.Equal(1, responseBody.PageSize);
            Assert.Equal(2, responseBody.TotalCount);
            Assert.Equal(2, responseBody.TotalPages);

            ListProductItemResponse item =
                Assert.Single(responseBody.Items);

            Assert.Equal(
                inactiveChairB.Id,
                item.ProductId);

            Assert.Equal(
                ProductStatus.Inactive,
                item.Status);

            Assert.Equal(
                inactiveChairB.LastModifiedAt,
                item.LastModifiedAt);
        }
        finally
        {
            await DeleteProductsAsync(
                dbContext,
                organisationId);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Get_WithMissingOrInvalidOrganisationHeader_ReturnsBadRequest(
        string? organisationHeader)
    {
        using HttpClient client =
            factory.CreateClient();

        if (organisationHeader is not null)
        {
            client.DefaultRequestHeaders.Add(
                "X-Organisation-Id",
                organisationHeader);
        }

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            Guid.NewGuid().ToString());

        HttpResponseMessage response =
            await client.GetAsync(
                "/api/v1/catalog/products");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers
                .ContentType?.MediaType);

        ValidationProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);

        Assert.True(
            problem.Errors.ContainsKey(
                "X-Organisation-Id"));

        string message =
            Assert.Single(
                problem.Errors["X-Organisation-Id"]);

        Assert.Equal(
            "Header 'X-Organisation-Id' must contain a non-empty GUID.",
            message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Get_WithMissingOrInvalidUserHeader_ReturnsBadRequest(
        string? userHeader)
    {
        using HttpClient client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            Guid.NewGuid().ToString());

        if (userHeader is not null)
        {
            client.DefaultRequestHeaders.Add(
                "X-User-Id",
                userHeader);
        }

        HttpResponseMessage response =
            await client.GetAsync(
                "/api/v1/catalog/products");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        ValidationProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);

        Assert.True(
            problem.Errors.ContainsKey(
                "X-User-Id"));

        string message =
            Assert.Single(
                problem.Errors["X-User-Id"]);

        Assert.Equal(
            "Header 'X-User-Id' must contain a non-empty GUID.",
            message);
    }

    [Theory]
    [InlineData(
        "pageNumber=0",
        "LIST_PRODUCTS_PAGE_NUMBER_INVALID",
        "Page number must be greater than or equal to 1.")]
    [InlineData(
        "pageSize=0",
        "LIST_PRODUCTS_PAGE_SIZE_INVALID",
        "Page size must be between 1 and 100.")]
    [InlineData(
        "pageSize=101",
        "LIST_PRODUCTS_PAGE_SIZE_INVALID",
        "Page size must be between 1 and 100.")]
    public async Task Get_WithInvalidPagination_ReturnsBadRequestProblemDetails(
        string queryString,
        string expectedCode,
        string expectedMessage)
    {
        using HttpClient client =
            CreateAuthenticatedClient(
                Guid.NewGuid());

        HttpResponseMessage response =
            await client.GetAsync(
                $"/api/v1/catalog/products?{queryString}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers
                .ContentType?.MediaType);

        ProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);

        Assert.Equal(
            StatusCodes.Status400BadRequest,
            problem.Status);

        Assert.Equal(
            "Request validation failed",
            problem.Title);

        Assert.Equal(
            expectedMessage,
            problem.Detail);

        Assert.Equal(
            "/api/v1/catalog/products",
            problem.Instance);

        Assert.True(
            problem.Extensions.TryGetValue(
                "code",
                out object? codeValue));

        JsonElement codeElement =
            Assert.IsType<JsonElement>(codeValue);

        Assert.Equal(
            expectedCode,
            codeElement.GetString());

        Assert.True(
            problem.Extensions.TryGetValue(
                "traceId",
                out object? traceIdValue));

        JsonElement traceIdElement =
            Assert.IsType<JsonElement>(
                traceIdValue);

        Assert.False(
            string.IsNullOrWhiteSpace(
                traceIdElement.GetString()));
    }

    [Fact]
    public async Task Get_WithSearchOverMaximumLength_ReturnsBadRequest()
    {
        using HttpClient client =
            CreateAuthenticatedClient(
                Guid.NewGuid());

        string search =
            new('A', 101);

        HttpResponseMessage response =
            await client.GetAsync(
                "/api/v1/catalog/products"
                + $"?search={search}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        ProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);

        Assert.Equal(
            "Request validation failed",
            problem.Title);

        Assert.Equal(
            "Search must not exceed 100 characters.",
            problem.Detail);

        Assert.True(
            problem.Extensions.TryGetValue(
                "code",
                out object? codeValue));

        JsonElement codeElement =
            Assert.IsType<JsonElement>(codeValue);

        Assert.Equal(
            "LIST_PRODUCTS_SEARCH_INVALID",
            codeElement.GetString());
    }

    [Fact]
    public async Task Get_WithNoProducts_ReturnsEmptyOkResponse()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        using HttpClient client =
            CreateAuthenticatedClient(
                Guid.NewGuid());

        HttpResponseMessage response =
            await client.GetAsync(
                "/api/v1/catalog/products");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        ListProductsResponse? responseBody =
            await response.Content
                .ReadFromJsonAsync<ListProductsResponse>();

        Assert.NotNull(responseBody);
        Assert.Empty(responseBody.Items);
        Assert.Equal(1, responseBody.PageNumber);
        Assert.Equal(20, responseBody.PageSize);
        Assert.Equal(0, responseBody.TotalCount);
        Assert.Equal(0, responseBody.TotalPages);
    }

    private HttpClient CreateAuthenticatedClient(
        Guid organisationId)
    {
        HttpClient client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            organisationId.ToString());

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            Guid.NewGuid().ToString());

        return client;
    }

    private static Product CreateProduct(
        Guid organisationId,
        string sku,
        string name)
    {
        return Product.Create(
            organisationId: organisationId,
            sku: sku,
            name: name,
            description: string.Empty,
            unitOfMeasureId: EachUnitOfMeasureId,
            productCategoryId: null,
            taxCategoryId: StandardGstTaxCategoryId,
            createdAt:
                DateTimeOffset.UtcNow.AddMinutes(-5),
            createdBy: Guid.NewGuid());
    }

    private static string CreateSuffix()
    {
        return Guid.NewGuid()
            .ToString("N")[..8]
            .ToUpperInvariant();
    }

    private static async Task DeleteProductsAsync(
        CatalogDbContext dbContext,
        params Guid[] organisationIds)
    {
        await dbContext.Products
            .Where(product =>
                organisationIds.Contains(
                    product.OrganisationId))
            .ExecuteDeleteAsync();
    }
}
