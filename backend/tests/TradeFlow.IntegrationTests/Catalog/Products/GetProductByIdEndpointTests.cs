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

public sealed class GetProductByIdEndpointTests(
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
    public async Task Get_WithExistingProduct_ReturnsOkProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid createdBy = Guid.NewGuid();

        DateTimeOffset createdAt =
            new(
                year: 2026,
                month: 8,
                day: 12,
                hour: 10,
                minute: 0,
                second: 0,
                offset: TimeSpan.Zero);

        Product product =
            CreateProduct(
                organisationId,
                createdAt,
                createdBy);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        using HttpClient client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            organisationId.ToString());

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            userId.ToString());

        try
        {
            HttpResponseMessage response =
                await client.GetAsync(
                    $"/api/v1/catalog/products/{product.Id}");

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            Assert.Equal(
                "application/json",
                response.Content.Headers.ContentType?.MediaType);

            GetProductByIdResponse? responseBody =
                await response.Content
                    .ReadFromJsonAsync<GetProductByIdResponse>();

            Assert.NotNull(responseBody);

            Assert.Equal(
                product.Id,
                responseBody.ProductId);

            Assert.Equal(
                product.Sku,
                responseBody.Sku);

            Assert.Equal(
                product.Name,
                responseBody.Name);

            Assert.Equal(
                product.Description,
                responseBody.Description);

            Assert.Equal(
                product.UnitOfMeasureId,
                responseBody.UnitOfMeasureId);

            Assert.Equal(
                product.ProductCategoryId,
                responseBody.ProductCategoryId);

            Assert.Equal(
                product.TaxCategoryId,
                responseBody.TaxCategoryId);

            Assert.Equal(
                ProductStatus.Active,
                responseBody.Status);

            Assert.Equal(
                product.CreatedAt,
                responseBody.CreatedAt);

            Assert.Equal(
                product.CreatedBy,
                responseBody.CreatedBy);

            Assert.Null(responseBody.LastModifiedAt);
            Assert.Null(responseBody.LastModifiedBy);

            Assert.Equal(
                Convert.ToBase64String(product.RowVersion),
                responseBody.RowVersion);

            Assert.NotEmpty(responseBody.RowVersion);
        }
        finally
        {
            await DeleteProductAsync(
                dbContext,
                product.Id);
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
                $"/api/v1/catalog/products/{Guid.NewGuid()}");

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
                $"/api/v1/catalog/products/{Guid.NewGuid()}");

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
                "X-User-Id"));

        string errorMessage =
            Assert.Single(
                problem.Errors["X-User-Id"]);

        Assert.Equal(
            "Header 'X-User-Id' must contain a non-empty GUID.",
            errorMessage);
    }

    [Fact]
    public async Task Get_WithUnknownProduct_ReturnsNotFoundProblemDetails()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        using HttpClient client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            Guid.NewGuid().ToString());

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            Guid.NewGuid().ToString());

        Guid productId = Guid.NewGuid();

        HttpResponseMessage response =
            await client.GetAsync(
                $"/api/v1/catalog/products/{productId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        ProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);

        Assert.Equal(
            StatusCodes.Status404NotFound,
            problem.Status);

        Assert.Equal(
            "Resource not found",
            problem.Title);

        Assert.Equal(
            "The requested product was not found.",
            problem.Detail);

        Assert.Equal(
            $"/api/v1/catalog/products/{productId}",
            problem.Instance);

        Assert.True(
            problem.Extensions.TryGetValue(
                "code",
                out object? codeValue));

        JsonElement codeElement =
            Assert.IsType<JsonElement>(codeValue);

        Assert.Equal(
            "PRODUCT_NOT_FOUND",
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

    [Fact]
    public async Task Get_WithProductFromAnotherOrganisation_ReturnsNotFound()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid owningOrganisationId = Guid.NewGuid();
        Guid requestingOrganisationId = Guid.NewGuid();

        Product product =
            CreateProduct(
                owningOrganisationId,
                DateTimeOffset.UtcNow,
                Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        using HttpClient client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            requestingOrganisationId.ToString());

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            Guid.NewGuid().ToString());

        try
        {
            HttpResponseMessage response =
                await client.GetAsync(
                    $"/api/v1/catalog/products/{product.Id}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);

            ProblemDetails? problem =
                await response.Content
                    .ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problem);

            Assert.Equal(
                StatusCodes.Status404NotFound,
                problem.Status);

            Assert.Equal(
                "The requested product was not found.",
                problem.Detail);

            Assert.True(
                problem.Extensions.TryGetValue(
                    "code",
                    out object? codeValue));

            JsonElement codeElement =
                Assert.IsType<JsonElement>(codeValue);

            Assert.Equal(
                "PRODUCT_NOT_FOUND",
                codeElement.GetString());
        }
        finally
        {
            await DeleteProductAsync(
                dbContext,
                product.Id);
        }
    }

    [Fact]
    public async Task Get_WithInactiveProduct_ReturnsOkProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid modifiedBy = Guid.NewGuid();

        DateTimeOffset createdAt =
            DateTimeOffset.UtcNow.AddMinutes(-2);

        DateTimeOffset modifiedAt =
            DateTimeOffset.UtcNow.AddMinutes(-1);

        Product product =
            CreateProduct(
                organisationId,
                createdAt,
                Guid.NewGuid());

        product.Deactivate(
            modifiedAt,
            modifiedBy);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        using HttpClient client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            organisationId.ToString());

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            Guid.NewGuid().ToString());

        try
        {
            HttpResponseMessage response =
                await client.GetAsync(
                    $"/api/v1/catalog/products/{product.Id}");

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            GetProductByIdResponse? responseBody =
                await response.Content
                    .ReadFromJsonAsync<GetProductByIdResponse>();

            Assert.NotNull(responseBody);

            Assert.Equal(
                ProductStatus.Inactive,
                responseBody.Status);

            Assert.Equal(
                modifiedAt.ToUniversalTime(),
                responseBody.LastModifiedAt);

            Assert.Equal(
                modifiedBy,
                responseBody.LastModifiedBy);
        }
        finally
        {
            await DeleteProductAsync(
                dbContext,
                product.Id);
        }
    }

    private static Product CreateProduct(
        Guid organisationId,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        return Product.Create(
            organisationId: organisationId,
            sku: $"ENDPOINT-{Guid.NewGuid():N}",
            name: "Office Chair",
            description: "Ergonomic office chair",
            unitOfMeasureId: EachUnitOfMeasureId,
            productCategoryId: null,
            taxCategoryId: StandardGstTaxCategoryId,
            createdAt: createdAt,
            createdBy: createdBy);
    }

    private static async Task DeleteProductAsync(
        CatalogDbContext dbContext,
        Guid productId)
    {
        await dbContext.Products
            .Where(product =>
                product.Id == productId)
            .ExecuteDeleteAsync();
    }
}
