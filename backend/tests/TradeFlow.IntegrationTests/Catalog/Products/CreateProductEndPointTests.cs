using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Api.Endpoints.Catalog.Products;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.Products;

public sealed class CreateProductEndpointTests(
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
    public async Task Post_WithValidRequest_ReturnsCreatedProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        using HttpClient client =
            factory.CreateClient();

        Guid organisationId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        string inputSku =
            $"chair-{Guid.NewGuid():N}";

        string expectedSku =
            inputSku.Trim().ToUpperInvariant();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            organisationId.ToString());

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            userId.ToString());

        CreateProductRequest request = new(
            Sku: inputSku,
            Name: "Office Chair",
            Description: "Ergonomic office chair.",
            UnitOfMeasureId: EachUnitOfMeasureId,
            ProductCategoryId: null,
            TaxCategoryId: StandardGstTaxCategoryId);

        try
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    "/api/v1/catalog/products",
                    request);

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);

            CreateProductResponse? responseBody =
                await response.Content
                    .ReadFromJsonAsync<CreateProductResponse>();

            Assert.NotNull(responseBody);
            Assert.NotEqual(
                Guid.Empty,
                responseBody.ProductId);

            Assert.Equal(
                expectedSku,
                responseBody.Sku);

            Assert.NotNull(response.Headers.Location);

            Assert.Equal(
                $"/api/v1/catalog/products/{responseBody.ProductId}",
                response.Headers.Location.ToString());

            Product? savedProduct =
                await dbContext.Products
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        product =>
                            product.Id ==
                                responseBody.ProductId);

            Assert.NotNull(savedProduct);
            Assert.Equal(
                organisationId,
                savedProduct.OrganisationId);

            Assert.Equal(
                userId,
                savedProduct.CreatedBy);

            Assert.Equal(
                expectedSku,
                savedProduct.Sku);

            Assert.Equal(
                EachUnitOfMeasureId,
                savedProduct.UnitOfMeasureId);

            Assert.Equal(
                StandardGstTaxCategoryId,
                savedProduct.TaxCategoryId);

            Assert.Equal(
                ProductStatus.Active,
                savedProduct.Status);
        }
        finally
        {
            await dbContext.Products
                .Where(product =>
                    product.OrganisationId ==
                        organisationId)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task Post_WithoutOrganisationIdHeader_ReturnsBadRequest()
    {
        using HttpClient client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            Guid.NewGuid().ToString());

        CreateProductRequest request = new(
            Sku: $"CHAIR-{Guid.NewGuid():N}",
            Name: "Office Chair",
            Description: string.Empty,
            UnitOfMeasureId: EachUnitOfMeasureId,
            ProductCategoryId: null,
            TaxCategoryId: StandardGstTaxCategoryId);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                "/api/v1/catalog/products",
                request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        ValidationProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);

        Assert.True(
            problem.Errors.ContainsKey(
                "X-Organisation-Id"));

        string errorMessage =
            Assert.Single(
                problem.Errors["X-Organisation-Id"]);

        Assert.Equal(
            "Header 'X-Organisation-Id' must contain a non-empty GUID.",
            errorMessage);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Post_WithInvalidOrganisationIdHeader_ReturnsBadRequest(
    string organisationIdHeader)
    {
        using HttpClient client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            organisationIdHeader);

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            Guid.NewGuid().ToString());

        CreateProductRequest request = new(
            Sku: $"CHAIR-{Guid.NewGuid():N}",
            Name: "Office Chair",
            Description: string.Empty,
            UnitOfMeasureId: EachUnitOfMeasureId,
            ProductCategoryId: null,
            TaxCategoryId: StandardGstTaxCategoryId);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                "/api/v1/catalog/products",
                request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        ValidationProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);

        Assert.True(
            problem.Errors.ContainsKey(
                "X-Organisation-Id"));

        string errorMessage =
            Assert.Single(
                problem.Errors["X-Organisation-Id"]);

        Assert.Equal(
            "Header 'X-Organisation-Id' must contain a non-empty GUID.",
            errorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Post_WithMissingOrInvalidUserIdHeader_ReturnsBadRequest(
    string? userIdHeader)
    {
        using HttpClient client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            Guid.NewGuid().ToString());

        if (userIdHeader is not null)
        {
            client.DefaultRequestHeaders.Add(
                "X-User-Id",
                userIdHeader);
        }

        CreateProductRequest request = new(
            Sku: $"CHAIR-{Guid.NewGuid():N}",
            Name: "Office Chair",
            Description: string.Empty,
            UnitOfMeasureId: EachUnitOfMeasureId,
            ProductCategoryId: null,
            TaxCategoryId: StandardGstTaxCategoryId);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                "/api/v1/catalog/products",
                request);

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

        string errorMessage =
            Assert.Single(
                problem.Errors["X-User-Id"]);

        Assert.Equal(
            "Header 'X-User-Id' must contain a non-empty GUID.",
            errorMessage);
    }

    [Fact]
    public async Task Post_WithDuplicateSku_ReturnsConflictProblemDetails()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        using HttpClient client =
            factory.CreateClient();

        Guid organisationId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            organisationId.ToString());

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            userId.ToString());

        string sku =
            $"DESK-{Guid.NewGuid():N}";

        CreateProductRequest firstRequest = new(
            Sku: sku,
            Name: "Office Desk",
            Description: string.Empty,
            UnitOfMeasureId: EachUnitOfMeasureId,
            ProductCategoryId: null,
            TaxCategoryId: StandardGstTaxCategoryId);

        CreateProductRequest duplicateRequest = new(
            Sku: $" {sku.ToLowerInvariant()} ",
            Name: "Another Office Desk",
            Description: string.Empty,
            UnitOfMeasureId: EachUnitOfMeasureId,
            ProductCategoryId: null,
            TaxCategoryId: StandardGstTaxCategoryId);

        try
        {
            HttpResponseMessage firstResponse =
                await client.PostAsJsonAsync(
                    "/api/v1/catalog/products",
                    firstRequest);

            Assert.Equal(
                HttpStatusCode.Created,
                firstResponse.StatusCode);

            HttpResponseMessage duplicateResponse =
                await client.PostAsJsonAsync(
                    "/api/v1/catalog/products",
                    duplicateRequest);

            Assert.Equal(
                HttpStatusCode.Conflict,
                duplicateResponse.StatusCode);

            Assert.Equal(
                "application/problem+json",
                duplicateResponse.Content.Headers
                    .ContentType?.MediaType);

            ProblemDetails? problem =
                await duplicateResponse.Content
                    .ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problem);

            Assert.Equal(
                StatusCodes.Status409Conflict,
                problem.Status);

            Assert.Equal(
                "Business rule conflict",
                problem.Title);

            Assert.Equal(
                "A product with this SKU already exists in the organisation.",
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
                "PRODUCT_SKU_ALREADY_EXISTS",
                codeElement.GetString());

            Assert.True(
                problem.Extensions.TryGetValue(
                    "traceId",
                    out object? traceIdValue));

            JsonElement traceIdElement =
                Assert.IsType<JsonElement>(traceIdValue);

            Assert.False(
                string.IsNullOrWhiteSpace(
                    traceIdElement.GetString()));

            int savedProductCount =
                await dbContext.Products
                    .AsNoTracking()
                    .CountAsync(
                        product =>
                            product.OrganisationId ==
                                organisationId &&
                            product.Sku ==
                                sku.ToUpperInvariant());

            Assert.Equal(1, savedProductCount);
        }
        finally
        {
            await dbContext.Products
                .Where(product =>
                    product.OrganisationId ==
                        organisationId)
                .ExecuteDeleteAsync();
        }
    }
    [Fact]
    public async Task Post_WithMalformedJson_ReturnsBadRequest()
    {
        using HttpClient client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            Guid.NewGuid().ToString());

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            Guid.NewGuid().ToString());

        using StringContent malformedJson = new(
            content: """
                 {
                   "sku": "CHAIR-001",
                   "name":
                 }
                 """,
            encoding: Encoding.UTF8,
            mediaType: "application/json");

        HttpResponseMessage response =
            await client.PostAsync(
                "/api/v1/catalog/products",
                malformedJson);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Post_WithEmptySku_ReturnsConflictProblemDetails(
    string? sku)
    {
        using HttpClient client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            Guid.NewGuid().ToString());

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            Guid.NewGuid().ToString());

        CreateProductRequest request = new(
            Sku: sku!,
            Name: "Office Chair",
            Description: string.Empty,
            UnitOfMeasureId: EachUnitOfMeasureId,
            ProductCategoryId: null,
            TaxCategoryId: StandardGstTaxCategoryId);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                "/api/v1/catalog/products",
                request);

        Assert.Equal(
            HttpStatusCode.Conflict,
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
            StatusCodes.Status409Conflict,
            problem.Status);

        Assert.Equal(
            "Business rule conflict",
            problem.Title);

        Assert.Equal(
            "Product SKU is required.",
            problem.Detail);

        Assert.True(
            problem.Extensions.TryGetValue(
                "code",
                out object? codeValue));

        JsonElement codeElement =
            Assert.IsType<JsonElement>(codeValue);

        Assert.Equal(
            "PRODUCT_SKU_REQUIRED",
            codeElement.GetString());

        Assert.True(
            problem.Extensions.TryGetValue(
                "traceId",
                out object? traceIdValue));

        JsonElement traceIdElement =
            Assert.IsType<JsonElement>(traceIdValue);

        Assert.False(
            string.IsNullOrWhiteSpace(
                traceIdElement.GetString()));
    }

}
