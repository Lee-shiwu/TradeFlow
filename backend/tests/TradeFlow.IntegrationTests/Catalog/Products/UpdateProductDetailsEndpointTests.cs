using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Api.Endpoints.Catalog.Products;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.Products;

public sealed class UpdateProductDetailsEndpointTests(
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
    public async Task Put_WithValidRequest_ReturnsUpdatedProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid createdBy = Guid.NewGuid();
        Guid modifiedBy = Guid.NewGuid();

        ProductCategory category =
            CreateProductCategory(
                organisationId,
                createdBy);

        Product product =
            CreateProduct(
                organisationId,
                productCategoryId: null,
                createdBy);

        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        byte[] originalRowVersion =
            product.RowVersion.ToArray();

        string originalRowVersionBase64 =
            Convert.ToBase64String(
                originalRowVersion);

        using HttpClient client =
            CreateClient(
                organisationId,
                modifiedBy);

        UpdateProductDetailsRequest request =
            new(
                Name: "  Updated Office Chair  ",
                Description: "  Updated description.  ",
                ProductCategoryId: category.Id,
                TaxCategoryId:
                    StandardGstTaxCategoryId,
                RowVersion:
                    originalRowVersionBase64);

        DateTimeOffset beforeRequest =
            DateTimeOffset.UtcNow;

        try
        {
            HttpResponseMessage response =
                await client.PutAsJsonAsync(
                    $"/api/v1/catalog/products/{product.Id}",
                    request);

            DateTimeOffset afterRequest =
                DateTimeOffset.UtcNow;

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            Assert.Equal(
                "application/json",
                response.Content.Headers
                    .ContentType?.MediaType);

            UpdateProductDetailsResponse? responseBody =
                await response.Content
                    .ReadFromJsonAsync<
                        UpdateProductDetailsResponse>();

            Assert.NotNull(responseBody);

            Assert.Equal(
                product.Id,
                responseBody.ProductId);

            Assert.Equal(
                "Updated Office Chair",
                responseBody.Name);

            Assert.Equal(
                "Updated description.",
                responseBody.Description);

            Assert.Equal(
                category.Id,
                responseBody.ProductCategoryId);

            Assert.Equal(
                StandardGstTaxCategoryId,
                responseBody.TaxCategoryId);

            Assert.Equal(
                modifiedBy,
                responseBody.LastModifiedBy);

            Assert.NotNull(
                responseBody.LastModifiedAt);

            Assert.InRange(
                responseBody.LastModifiedAt.Value,
                beforeRequest.AddSeconds(-1),
                afterRequest.AddSeconds(1));

            byte[] returnedRowVersion =
                Convert.FromBase64String(
                    responseBody.RowVersion);

            Assert.NotEmpty(returnedRowVersion);

            Assert.False(
                originalRowVersion.SequenceEqual(
                    returnedRowVersion));

            dbContext.ChangeTracker.Clear();

            Product? savedProduct =
                await dbContext.Products
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        existingProduct =>
                            existingProduct.Id ==
                                product.Id);

            Assert.NotNull(savedProduct);

            Assert.Equal(
                "Updated Office Chair",
                savedProduct.Name);

            Assert.Equal(
                "Updated description.",
                savedProduct.Description);

            Assert.Equal(
                category.Id,
                savedProduct.ProductCategoryId);

            Assert.Equal(
                modifiedBy,
                savedProduct.LastModifiedBy);

            Assert.Equal(
                returnedRowVersion,
                savedProduct.RowVersion);
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                product.Id,
                category.Id);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    [InlineData(
        "00000000-0000-0000-0000-000000000000")]
    public async Task Put_WithMissingOrInvalidOrganisationHeader_ReturnsBadRequest(
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

        UpdateProductDetailsRequest request =
            CreateRequest(
                Convert.ToBase64String(
                    new byte[8]));

        HttpResponseMessage response =
            await client.PutAsJsonAsync(
                $"/api/v1/catalog/products/{Guid.NewGuid()}",
                request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers
                .ContentType?.MediaType);

        ValidationProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<
                    ValidationProblemDetails>();

        Assert.NotNull(problem);

        Assert.True(
            problem.Errors.ContainsKey(
                "X-Organisation-Id"));

        string message =
            Assert.Single(
                problem.Errors[
                    "X-Organisation-Id"]);

        Assert.Equal(
            "Header 'X-Organisation-Id' must contain a non-empty GUID.",
            message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    [InlineData(
        "00000000-0000-0000-0000-000000000000")]
    public async Task Put_WithMissingOrInvalidUserHeader_ReturnsBadRequest(
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

        UpdateProductDetailsRequest request =
            CreateRequest(
                Convert.ToBase64String(
                    new byte[8]));

        HttpResponseMessage response =
            await client.PutAsJsonAsync(
                $"/api/v1/catalog/products/{Guid.NewGuid()}",
                request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        ValidationProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<
                    ValidationProblemDetails>();

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
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-valid-base64")]
    public async Task Put_WithInvalidRowVersion_ReturnsBadRequest(
        string? rowVersion)
    {
        using HttpClient client =
            CreateClient(
                Guid.NewGuid(),
                Guid.NewGuid());

        UpdateProductDetailsRequest request =
            new(
                Name: "Updated Product",
                Description: string.Empty,
                ProductCategoryId: null,
                TaxCategoryId:
                    StandardGstTaxCategoryId,
                RowVersion: rowVersion!);

        HttpResponseMessage response =
            await client.PutAsJsonAsync(
                $"/api/v1/catalog/products/{Guid.NewGuid()}",
                request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers
                .ContentType?.MediaType);

        ValidationProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<
                    ValidationProblemDetails>();

        Assert.NotNull(problem);

        Assert.True(
            problem.Errors.ContainsKey(
                "RowVersion"));

        string message =
            Assert.Single(
                problem.Errors["RowVersion"]);

        Assert.Equal(
            "RowVersion must be a valid non-empty Base64 value.",
            message);
    }

    [Fact]
    public async Task Put_WithUnknownProduct_ReturnsNotFoundProblemDetails()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();

        using HttpClient client =
            CreateClient(
                organisationId,
                Guid.NewGuid());

        UpdateProductDetailsRequest request =
            CreateRequest(
                Convert.ToBase64String(
                    new byte[8]));

        HttpResponseMessage response =
            await client.PutAsJsonAsync(
                $"/api/v1/catalog/products/{productId}",
                request);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers
                .ContentType?.MediaType);

        ProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<
                    ProblemDetails>();

        Assert.NotNull(problem);

        Assert.Equal(
            StatusCodes.Status404NotFound,
            problem.Status);

        Assert.Equal(
            "Resource not found",
            problem.Title);

        Assert.Equal(
            "The selected product does not exist.",
            problem.Detail);

        Assert.Equal(
            "UPDATE_PRODUCT_NOT_FOUND",
            GetProblemCode(problem));
    }

    [Fact]
    public async Task Put_WithProductFromAnotherOrganisation_ReturnsNotFound()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid owningOrganisationId =
            Guid.NewGuid();

        Guid requestingOrganisationId =
            Guid.NewGuid();

        Product product =
            CreateProduct(
                owningOrganisationId,
                productCategoryId: null,
                createdBy: Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        string rowVersion =
            Convert.ToBase64String(
                product.RowVersion);

        using HttpClient client =
            CreateClient(
                requestingOrganisationId,
                Guid.NewGuid());

        UpdateProductDetailsRequest request =
            CreateRequest(rowVersion);

        try
        {
            HttpResponseMessage response =
                await client.PutAsJsonAsync(
                    $"/api/v1/catalog/products/{product.Id}",
                    request);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);

            ProblemDetails? problem =
                await response.Content
                    .ReadFromJsonAsync<
                        ProblemDetails>();

            Assert.NotNull(problem);

            Assert.Equal(
                "UPDATE_PRODUCT_NOT_FOUND",
                GetProblemCode(problem));

            dbContext.ChangeTracker.Clear();

            Product? savedProduct =
                await dbContext.Products
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        existingProduct =>
                            existingProduct.Id ==
                                product.Id);

            Assert.NotNull(savedProduct);

            Assert.Equal(
                "Office Chair",
                savedProduct.Name);
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                product.Id);
        }
    }

    [Fact]
    public async Task Put_WithCategoryFromAnotherOrganisation_ReturnsConflict()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid productOrganisationId =
            Guid.NewGuid();

        Guid categoryOrganisationId =
            Guid.NewGuid();

        Guid userId = Guid.NewGuid();

        ProductCategory category =
            CreateProductCategory(
                categoryOrganisationId,
                userId);

        Product product =
            CreateProduct(
                productOrganisationId,
                productCategoryId: null,
                createdBy: userId);

        dbContext.ProductCategories.Add(category);
        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        using HttpClient client =
            CreateClient(
                productOrganisationId,
                userId);

        UpdateProductDetailsRequest request =
            new(
                Name: "Updated Product",
                Description: string.Empty,
                ProductCategoryId: category.Id,
                TaxCategoryId:
                    StandardGstTaxCategoryId,
                RowVersion:
                    Convert.ToBase64String(
                        product.RowVersion));

        try
        {
            HttpResponseMessage response =
                await client.PutAsJsonAsync(
                    $"/api/v1/catalog/products/{product.Id}",
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
                    .ReadFromJsonAsync<
                        ProblemDetails>();

            Assert.NotNull(problem);

            Assert.Equal(
                StatusCodes.Status409Conflict,
                problem.Status);

            Assert.Equal(
                "Business rule conflict",
                problem.Title);

            Assert.Equal(
                "UPDATE_PRODUCT_CATEGORY_NOT_FOUND",
                GetProblemCode(problem));
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                product.Id,
                category.Id);
        }
    }

    [Fact]
    public async Task Put_WithUnknownTaxCategory_ReturnsConflict()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();

        Product product =
            CreateProduct(
                organisationId,
                productCategoryId: null,
                createdBy: Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        using HttpClient client =
            CreateClient(
                organisationId,
                Guid.NewGuid());

        UpdateProductDetailsRequest request =
            new(
                Name: "Updated Product",
                Description: string.Empty,
                ProductCategoryId: null,
                TaxCategoryId: Guid.NewGuid(),
                RowVersion:
                    Convert.ToBase64String(
                        product.RowVersion));

        try
        {
            HttpResponseMessage response =
                await client.PutAsJsonAsync(
                    $"/api/v1/catalog/products/{product.Id}",
                    request);

            Assert.Equal(
                HttpStatusCode.Conflict,
                response.StatusCode);

            ProblemDetails? problem =
                await response.Content
                    .ReadFromJsonAsync<
                        ProblemDetails>();

            Assert.NotNull(problem);

            Assert.Equal(
                "UPDATE_PRODUCT_TAX_CATEGORY_NOT_FOUND",
                GetProblemCode(problem));
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                product.Id);
        }
    }

    [Fact]
    public async Task Put_WithStaleRowVersion_ReturnsConflictAndDoesNotOverwriteProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();

        Product product =
            CreateProduct(
                organisationId,
                productCategoryId: null,
                createdBy: Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        byte[] staleRowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            await dbContext.Products
                .Where(existingProduct =>
                    existingProduct.Id == product.Id)
                .ExecuteUpdateAsync(
                    setters =>
                        setters.SetProperty(
                            existingProduct =>
                                existingProduct.Name,
                            "Changed By Another User"));

            dbContext.ChangeTracker.Clear();

            using HttpClient client =
                CreateClient(
                    organisationId,
                    Guid.NewGuid());

            UpdateProductDetailsRequest request =
                new(
                    Name: "My Stale Update",
                    Description:
                        "This must not be saved.",
                    ProductCategoryId: null,
                    TaxCategoryId:
                        StandardGstTaxCategoryId,
                    RowVersion:
                        Convert.ToBase64String(
                            staleRowVersion));

            HttpResponseMessage response =
                await client.PutAsJsonAsync(
                    $"/api/v1/catalog/products/{product.Id}",
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
                    .ReadFromJsonAsync<
                        ProblemDetails>();

            Assert.NotNull(problem);

            Assert.Equal(
                StatusCodes.Status409Conflict,
                problem.Status);

            Assert.Equal(
                "Business rule conflict",
                problem.Title);

            Assert.Equal(
                "The product was modified by another user. Reload the product and try again.",
                problem.Detail);

            Assert.Equal(
                "UPDATE_PRODUCT_CONCURRENCY_CONFLICT",
                GetProblemCode(problem));

            dbContext.ChangeTracker.Clear();

            Product? savedProduct =
                await dbContext.Products
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        existingProduct =>
                            existingProduct.Id ==
                                product.Id);

            Assert.NotNull(savedProduct);

            Assert.Equal(
                "Changed By Another User",
                savedProduct.Name);

            Assert.NotEqual(
                "My Stale Update",
                savedProduct.Name);
        }
        finally
        {
            await DeleteTestDataAsync(
                dbContext,
                product.Id);
        }
    }

    private HttpClient CreateClient(
        Guid organisationId,
        Guid userId)
    {
        HttpClient client =
            factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            organisationId.ToString());

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            userId.ToString());

        return client;
    }

    private static UpdateProductDetailsRequest CreateRequest(
        string rowVersion)
    {
        return new UpdateProductDetailsRequest(
            Name: "Updated Product",
            Description: "Updated description.",
            ProductCategoryId: null,
            TaxCategoryId:
                StandardGstTaxCategoryId,
            RowVersion: rowVersion);
    }

    private static Product CreateProduct(
        Guid organisationId,
        Guid? productCategoryId,
        Guid createdBy)
    {
        return Product.Create(
            organisationId: organisationId,
            sku: $"UPDATE-{Guid.NewGuid():N}",
            name: "Office Chair",
            description:
                "Ergonomic office chair.",
            unitOfMeasureId:
                EachUnitOfMeasureId,
            productCategoryId:
                productCategoryId,
            taxCategoryId:
                StandardGstTaxCategoryId,
            createdAt:
                DateTimeOffset.UtcNow.AddDays(-1),
            createdBy: createdBy);
    }

    private static ProductCategory CreateProductCategory(
        Guid organisationId,
        Guid createdBy)
    {
        return ProductCategory.Create(
            organisationId: organisationId,
            code: $"CATEGORY-{Guid.NewGuid():N}",
            name: "Office Furniture",
            description:
                "Office furniture category.",
            createdAt:
                DateTimeOffset.UtcNow.AddDays(-1),
            createdBy: createdBy);
    }

    private static string GetProblemCode(
        ProblemDetails problem)
    {
        Assert.True(
            problem.Extensions.TryGetValue(
                "code",
                out object? codeValue));

        JsonElement codeElement =
            Assert.IsType<JsonElement>(
                codeValue);

        string? code =
            codeElement.GetString();

        Assert.False(
            string.IsNullOrWhiteSpace(code));

        return code;
    }

    private static async Task DeleteTestDataAsync(
        CatalogDbContext dbContext,
        Guid productId,
        Guid? productCategoryId = null)
    {
        dbContext.ChangeTracker.Clear();

        await dbContext.Products
            .Where(product =>
                product.Id == productId)
            .ExecuteDeleteAsync();

        if (productCategoryId.HasValue)
        {
            await dbContext.ProductCategories
                .Where(category =>
                    category.Id ==
                        productCategoryId.Value)
                .ExecuteDeleteAsync();
        }
    }
}
