using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Api.Endpoints.Catalog.ProductCategories;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.ProductCategories;

public sealed class CreateProductCategoryEndpointTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Post_WithValidRequest_CreatesCategoryForCurrentIdentity()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        string code = $" api-{CreateSuffix()} ";

        using HttpClient client =
            CreateAuthenticatedClient(organisationId, userId);

        try
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    "/api/v1/catalog/product-categories",
                    new CreateProductCategoryRequest(
                        Code: code,
                        Name: "  Office   products  ",
                        Description: "  Products used in the office.  "));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            CreateProductCategoryResponse? body =
                await response.Content
                    .ReadFromJsonAsync<CreateProductCategoryResponse>();

            Assert.NotNull(body);
            Assert.Equal(code.Trim().ToUpperInvariant(), body.Code);
            Assert.Equal("Office products", body.Name);
            Assert.Equal("Products used in the office.", body.Description);
            Assert.Equal(ProductCategoryStatus.Active, body.Status);
            Assert.Equal(userId, body.CreatedBy);
            Assert.Null(body.LastModifiedAt);
            Assert.Null(body.LastModifiedBy);
            Assert.False(string.IsNullOrWhiteSpace(body.RowVersion));
            Assert.Equal(
                $"/api/v1/catalog/product-categories/{body.ProductCategoryId}",
                response.Headers.Location?.ToString());

            ProductCategory? persisted =
                await dbContext.ProductCategories
                    .AsNoTracking()
                    .SingleOrDefaultAsync(category =>
                        category.Id == body.ProductCategoryId);

            Assert.NotNull(persisted);
            Assert.Equal(organisationId, persisted.OrganisationId);
            Assert.Equal(userId, persisted.CreatedBy);
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task Post_WithDuplicateCode_ReturnsConflictProblem()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        string code = $"DUP-{CreateSuffix()}";

        dbContext.ProductCategories.Add(
            ProductCategory.Create(
                organisationId,
                code,
                "Existing category",
                null,
                DateTimeOffset.UtcNow.AddMinutes(-5),
                Guid.NewGuid()));
        await dbContext.SaveChangesAsync();

        using HttpClient client =
            CreateAuthenticatedClient(organisationId, Guid.NewGuid());

        try
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    "/api/v1/catalog/product-categories",
                    new CreateProductCategoryRequest(
                        Code: code.ToLowerInvariant(),
                        Name: "Duplicate category",
                        Description: null));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            ProblemDetails? problem =
                await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problem);

            JsonElement errorCode =
                Assert.IsType<JsonElement>(problem.Extensions["code"]);

            Assert.Equal(
                "PRODUCT_CATEGORY_CODE_ALREADY_EXISTS",
                errorCode.GetString());
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task Post_WithInvalidCode_ReturnsBusinessRuleProblem()
    {
        using HttpClient client =
            CreateAuthenticatedClient(Guid.NewGuid(), Guid.NewGuid());

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                "/api/v1/catalog/product-categories",
                new CreateProductCategoryRequest(
                    Code: "invalid code",
                    Name: "Invalid category",
                    Description: null));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        ProblemDetails? problem =
            await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);

        JsonElement errorCode =
            Assert.IsType<JsonElement>(problem.Extensions["code"]);

        Assert.Equal("PRODUCT_CATEGORY_CODE_INVALID", errorCode.GetString());
    }

    [Theory]
    [InlineData("X-Organisation-Id")]
    [InlineData("X-User-Id")]
    public async Task Post_WithMissingIdentityHeader_ReturnsBadRequest(
        string missingHeader)
    {
        using HttpClient client = factory.CreateClient();

        if (missingHeader != "X-Organisation-Id")
        {
            client.DefaultRequestHeaders.Add(
                "X-Organisation-Id",
                Guid.NewGuid().ToString());
        }

        if (missingHeader != "X-User-Id")
        {
            client.DefaultRequestHeaders.Add(
                "X-User-Id",
                Guid.NewGuid().ToString());
        }

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                "/api/v1/catalog/product-categories",
                new CreateProductCategoryRequest(
                    Code: $"CREATE-{CreateSuffix()}",
                    Name: "Office products",
                    Description: null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        ValidationProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Contains(missingHeader, problem.Errors.Keys);
    }

    private HttpClient CreateAuthenticatedClient(
        Guid organisationId,
        Guid userId)
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            organisationId.ToString());
        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            userId.ToString());
        return client;
    }

    private static string CreateSuffix()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }

    private static async Task DeleteCategoriesAsync(
        CatalogDbContext dbContext,
        params Guid[] organisationIds)
    {
        dbContext.ChangeTracker.Clear();

        await dbContext.ProductCategories
            .Where(category =>
                organisationIds.Contains(category.OrganisationId))
            .ExecuteDeleteAsync();
    }
}
