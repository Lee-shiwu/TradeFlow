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

public sealed class ProductCategoryStatusEndpointTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Theory]
    [InlineData("deactivate", ProductCategoryStatus.Inactive)]
    [InlineData("activate", ProductCategoryStatus.Active)]
    public async Task Post_WithValidRequest_ChangesCategoryStatus(
        string action,
        ProductCategoryStatus expectedStatus)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext = GetDbContext(scope);
        await dbContext.Database.MigrateAsync();
        Guid organisationId = Guid.NewGuid();
        Guid modifiedBy = Guid.NewGuid();
        ProductCategory category =
            CreateCategory(organisationId, inactive: action == "activate");
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();
        byte[] originalRowVersion = category.RowVersion.ToArray();
        using HttpClient client = CreateClient(organisationId, modifiedBy);

        try
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(
                $"/api/v1/catalog/product-categories/{category.Id}/{action}",
                new ProductCategoryStatusRequest(
                    Convert.ToBase64String(originalRowVersion)));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            ProductCategoryStatusResponse? body =
                await response.Content
                    .ReadFromJsonAsync<ProductCategoryStatusResponse>();
            Assert.NotNull(body);
            Assert.Equal(expectedStatus, body.Status);
            Assert.Equal(modifiedBy, body.LastModifiedBy);
            Assert.False(
                originalRowVersion.SequenceEqual(
                    Convert.FromBase64String(body.RowVersion)));
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task Post_WithInvalidRowVersion_ReturnsValidationProblem()
    {
        using HttpClient client = CreateClient(Guid.NewGuid(), Guid.NewGuid());

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/v1/catalog/product-categories/{Guid.NewGuid()}/deactivate",
            new ProductCategoryStatusRequest("not-base64"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails? problem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("RowVersion", problem.Errors.Keys);
    }

    [Fact]
    public async Task Post_WithStaleRowVersion_ReturnsConflictProblem()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext = GetDbContext(scope);
        await dbContext.Database.MigrateAsync();
        Guid organisationId = Guid.NewGuid();
        ProductCategory category = CreateCategory(organisationId);
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();
        using HttpClient client = CreateClient(organisationId, Guid.NewGuid());

        try
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(
                $"/api/v1/catalog/product-categories/{category.Id}/deactivate",
                new ProductCategoryStatusRequest(
                    Convert.ToBase64String(new byte[8])));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            ProblemDetails? problem =
                await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(problem);
            JsonElement code = Assert.IsType<JsonElement>(problem.Extensions["code"]);
            Assert.Equal(
                "DEACTIVATE_PRODUCT_CATEGORY_CONCURRENCY_CONFLICT",
                code.GetString());
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task Post_WithCategoryFromAnotherOrganisation_ReturnsNotFound()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext = GetDbContext(scope);
        await dbContext.Database.MigrateAsync();
        Guid ownerOrganisationId = Guid.NewGuid();
        ProductCategory category = CreateCategory(ownerOrganisationId);
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();
        using HttpClient client = CreateClient(Guid.NewGuid(), Guid.NewGuid());

        try
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(
                $"/api/v1/catalog/product-categories/{category.Id}/deactivate",
                new ProductCategoryStatusRequest(
                    Convert.ToBase64String(category.RowVersion)));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, ownerOrganisationId);
        }
    }

    private HttpClient CreateClient(Guid organisationId, Guid userId)
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            organisationId.ToString());
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        return client;
    }

    private static CatalogDbContext GetDbContext(IServiceScope scope)
    {
        return scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    }

    private static ProductCategory CreateCategory(
        Guid organisationId,
        bool inactive = false)
    {
        ProductCategory category = ProductCategory.Create(
            organisationId,
            $"API-{Guid.NewGuid():N}"[..12].ToUpperInvariant(),
            "API status category",
            null,
            DateTimeOffset.UtcNow.AddMinutes(-10),
            Guid.NewGuid());

        if (inactive)
        {
            category.Deactivate(DateTimeOffset.UtcNow.AddMinutes(-5), Guid.NewGuid());
        }

        return category;
    }

    private static async Task DeleteCategoriesAsync(
        CatalogDbContext dbContext,
        params Guid[] organisationIds)
    {
        dbContext.ChangeTracker.Clear();
        await dbContext.ProductCategories
            .Where(category => organisationIds.Contains(category.OrganisationId))
            .ExecuteDeleteAsync();
    }
}
