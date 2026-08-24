using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradeFlow.Api.Endpoints.Catalog.Products;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.Products;

public sealed class ActivateProductEndpointTests(
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
    public async Task Post_WithValidRequest_ActivatesProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid modifiedBy = Guid.NewGuid();

        Product product =
            CreateInactiveProduct(
                organisationId,
                Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        byte[] originalRowVersion =
            product.RowVersion.ToArray();

        using HttpClient client =
            CreateClient(
                organisationId,
                modifiedBy);

        ActivateProductRequest request =
            CreateRequest(originalRowVersion);

        DateTimeOffset beforeRequest =
            DateTimeOffset.UtcNow;

        try
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    $"/api/v1/catalog/products/" +
                    $"{product.Id}/activate",
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

            ActivateProductResponse? responseBody =
                await response.Content
                    .ReadFromJsonAsync<
                        ActivateProductResponse>();

            Assert.NotNull(responseBody);

            Assert.Equal(
                product.Id,
                responseBody.ProductId);

            Assert.Equal(
                ProductStatus.Active,
                responseBody.Status);

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
                ProductStatus.Active,
                savedProduct.Status);

            Assert.Equal(
                modifiedBy,
                savedProduct.LastModifiedBy);

            Assert.Equal(
                responseBody.LastModifiedAt,
                savedProduct.LastModifiedAt);

            Assert.Equal(
                returnedRowVersion,
                savedProduct.RowVersion);
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
    [InlineData(
        "00000000-0000-0000-0000-000000000000")]
    public async Task Post_WithMissingOrInvalidOrganisationHeader_ReturnsBadRequest(
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

        ActivateProductRequest request =
            CreateRequest([1]);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                $"/api/v1/catalog/products/" +
                $"{Guid.NewGuid()}/activate",
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
    public async Task Post_WithMissingOrInvalidUserHeader_ReturnsBadRequest(
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

        ActivateProductRequest request =
            CreateRequest([1]);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                $"/api/v1/catalog/products/" +
                $"{Guid.NewGuid()}/activate",
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
                "X-User-Id"));

        string message =
            Assert.Single(
                problem.Errors[
                    "X-User-Id"]);

        Assert.Equal(
            "Header 'X-User-Id' must contain a non-empty GUID.",
            message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-valid-base64")]
    public async Task Post_WithInvalidRowVersion_ReturnsBadRequest(
        string? rowVersion)
    {
        using HttpClient client =
            CreateClient(
                Guid.NewGuid(),
                Guid.NewGuid());

        ActivateProductRequest request =
            new(
                RowVersion: rowVersion);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                $"/api/v1/catalog/products/" +
                $"{Guid.NewGuid()}/activate",
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
    public async Task Post_WithUnknownProduct_ReturnsNotFoundProblemDetails()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        using HttpClient client =
            CreateClient(
                Guid.NewGuid(),
                Guid.NewGuid());

        Guid productId = Guid.NewGuid();

        ActivateProductRequest request =
            CreateRequest([1]);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                $"/api/v1/catalog/products/" +
                $"{productId}/activate",
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
                .ReadFromJsonAsync<ProblemDetails>();

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
            "ACTIVATE_PRODUCT_NOT_FOUND",
            GetProblemCode(problem));
    }

    [Fact]
    public async Task Post_WithProductFromAnotherOrganisation_ReturnsNotFound()
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
            CreateInactiveProduct(
                owningOrganisationId,
                Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        byte[] originalRowVersion =
            product.RowVersion.ToArray();

        using HttpClient client =
            CreateClient(
                requestingOrganisationId,
                Guid.NewGuid());

        ActivateProductRequest request =
            CreateRequest(originalRowVersion);

        try
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    $"/api/v1/catalog/products/" +
                    $"{product.Id}/activate",
                    request);

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);

            ProblemDetails? problem =
                await response.Content
                    .ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problem);

            Assert.Equal(
                "ACTIVATE_PRODUCT_NOT_FOUND",
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
                ProductStatus.Inactive,
                savedProduct.Status);

            Assert.True(
                originalRowVersion.SequenceEqual(
                    savedProduct.RowVersion));
        }
        finally
        {
            await DeleteProductAsync(
                dbContext,
                product.Id);
        }
    }

    [Fact]
    public async Task Post_WithStaleRowVersion_ReturnsConflictAndDoesNotActivateProduct()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();

        Product product =
            CreateInactiveProduct(
                organisationId,
                Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        byte[] staleRowVersion =
            product.RowVersion.ToArray();

        try
        {
            dbContext.ChangeTracker.Clear();

            await dbContext.Products
                .Where(existingProduct =>
                    existingProduct.Id ==
                        product.Id)
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

            ActivateProductRequest request =
                CreateRequest(staleRowVersion);

            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    $"/api/v1/catalog/products/" +
                    $"{product.Id}/activate",
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
                "The product was modified by another user. Reload the product and try again.",
                problem.Detail);

            Assert.Equal(
                "ACTIVATE_PRODUCT_CONCURRENCY_CONFLICT",
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
                ProductStatus.Inactive,
                savedProduct.Status);

            Assert.Equal(
                "Changed By Another User",
                savedProduct.Name);
        }
        finally
        {
            await DeleteProductAsync(
                dbContext,
                product.Id);
        }
    }

    [Fact]
    public async Task Post_WithAlreadyActiveProduct_ReturnsOkWithoutChangingAuditOrRowVersion()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();

        Product product =
            CreateActiveProduct(
                organisationId,
                Guid.NewGuid());

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        byte[] originalRowVersion =
            product.RowVersion.ToArray();

        using HttpClient client =
            CreateClient(
                organisationId,
                Guid.NewGuid());

        ActivateProductRequest request =
            CreateRequest(originalRowVersion);

        try
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    $"/api/v1/catalog/products/" +
                    $"{product.Id}/activate",
                    request);

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            ActivateProductResponse? responseBody =
                await response.Content
                    .ReadFromJsonAsync<
                        ActivateProductResponse>();

            Assert.NotNull(responseBody);

            Assert.Equal(
                ProductStatus.Active,
                responseBody.Status);

            Assert.Null(responseBody.LastModifiedAt);
            Assert.Null(responseBody.LastModifiedBy);

            byte[] returnedRowVersion =
                Convert.FromBase64String(
                    responseBody.RowVersion);

            Assert.True(
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
                ProductStatus.Active,
                savedProduct.Status);

            Assert.Null(savedProduct.LastModifiedAt);
            Assert.Null(savedProduct.LastModifiedBy);

            Assert.True(
                originalRowVersion.SequenceEqual(
                    savedProduct.RowVersion));
        }
        finally
        {
            await DeleteProductAsync(
                dbContext,
                product.Id);
        }
    }

    [Fact]
    public async Task Post_InProductionEnvironment_ReturnsNotImplemented()
    {
        using WebApplicationFactory<Program>
            productionFactory =
                factory.WithWebHostBuilder(
                    builder =>
                        builder.UseEnvironment(
                            Environments.Production));

        using HttpClient client =
            productionFactory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            Guid.NewGuid().ToString());

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            Guid.NewGuid().ToString());

        ActivateProductRequest request =
            CreateRequest([1]);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                $"/api/v1/catalog/products/" +
                $"{Guid.NewGuid()}/activate",
                request);

        Assert.Equal(
            HttpStatusCode.NotImplemented,
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
            StatusCodes.Status501NotImplemented,
            problem.Status);

        Assert.Equal(
            "Authentication is not configured.",
            problem.Title);
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

    private static ActivateProductRequest CreateRequest(
        byte[] rowVersion)
    {
        return new ActivateProductRequest(
            RowVersion:
                Convert.ToBase64String(
                    rowVersion));
    }

    private static Product CreateActiveProduct(
        Guid organisationId,
        Guid createdBy)
    {
        return Product.Create(
            organisationId: organisationId,
            sku:
                $"ACTIVATE-ENDPOINT-{Guid.NewGuid():N}",
            name: "Office Chair",
            description:
                "Ergonomic office chair.",
            unitOfMeasureId:
                EachUnitOfMeasureId,
            productCategoryId: null,
            taxCategoryId:
                StandardGstTaxCategoryId,
            createdAt:
                DateTimeOffset.UtcNow.AddDays(-2),
            createdBy: createdBy);
    }

    private static Product CreateInactiveProduct(
        Guid organisationId,
        Guid createdBy)
    {
        Product product =
            CreateActiveProduct(
                organisationId,
                createdBy);

        product.Deactivate(
            modifiedAt:
                DateTimeOffset.UtcNow.AddMinutes(-10),
            modifiedBy: createdBy);

        return product;
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

    private static async Task DeleteProductAsync(
        CatalogDbContext dbContext,
        Guid productId)
    {
        dbContext.ChangeTracker.Clear();

        await dbContext.Products
            .Where(product =>
                product.Id == productId)
            .ExecuteDeleteAsync();
    }
}
