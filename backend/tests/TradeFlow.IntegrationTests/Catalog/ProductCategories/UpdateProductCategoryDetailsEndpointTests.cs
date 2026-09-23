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

public sealed class UpdateProductCategoryDetailsEndpointTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Put_WithValidRequest_ReturnsUpdatedCategory()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid modifiedBy = Guid.NewGuid();
        ProductCategory category = CreateCategory(organisationId);
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();
        byte[] originalRowVersion = category.RowVersion.ToArray();

        using HttpClient client = CreateClient(organisationId, modifiedBy);

        try
        {
            HttpResponseMessage response = await client.PutAsJsonAsync(
                $"/api/v1/catalog/product-categories/{category.Id}",
                new UpdateProductCategoryDetailsRequest(
                    "  Updated   category  ",
                    "  Updated description.  ",
                    Convert.ToBase64String(originalRowVersion)));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            UpdateProductCategoryDetailsResponse? body =
                await response.Content
                    .ReadFromJsonAsync<UpdateProductCategoryDetailsResponse>();

            Assert.NotNull(body);
            Assert.Equal(category.Id, body.ProductCategoryId);
            Assert.Equal(category.Code, body.Code);
            Assert.Equal("Updated category", body.Name);
            Assert.Equal("Updated description.", body.Description);
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
    public async Task Put_WithInvalidBase64RowVersion_ReturnsValidationProblem()
    {
        using HttpClient client = CreateClient(Guid.NewGuid(), Guid.NewGuid());

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/v1/catalog/product-categories/{Guid.NewGuid()}",
            new UpdateProductCategoryDetailsRequest(
                "Category",
                null,
                "not-base64"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails? problem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("RowVersion", problem.Errors.Keys);
    }

    [Fact]
    public async Task Put_WithStaleRowVersion_ReturnsConflictProblem()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        ProductCategory category = CreateCategory(organisationId);
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();
        using HttpClient client = CreateClient(organisationId, Guid.NewGuid());

        try
        {
            HttpResponseMessage response = await client.PutAsJsonAsync(
                $"/api/v1/catalog/product-categories/{category.Id}",
                new UpdateProductCategoryDetailsRequest(
                    "Updated category",
                    null,
                    Convert.ToBase64String(new byte[8])));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            ProblemDetails? problem =
                await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(problem);
            JsonElement code = Assert.IsType<JsonElement>(problem.Extensions["code"]);
            Assert.Equal(
                "UPDATE_PRODUCT_CATEGORY_CONCURRENCY_CONFLICT",
                code.GetString());
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task Put_WithCategoryFromAnotherOrganisation_ReturnsNotFound()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await dbContext.Database.MigrateAsync();

        Guid ownerOrganisationId = Guid.NewGuid();
        ProductCategory category = CreateCategory(ownerOrganisationId);
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();
        using HttpClient client = CreateClient(Guid.NewGuid(), Guid.NewGuid());

        try
        {
            HttpResponseMessage response = await client.PutAsJsonAsync(
                $"/api/v1/catalog/product-categories/{category.Id}",
                new UpdateProductCategoryDetailsRequest(
                    "Updated category",
                    null,
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

    private static ProductCategory CreateCategory(Guid organisationId)
    {
        return ProductCategory.Create(
            organisationId,
            $"EDIT-{Guid.NewGuid():N}"[..13].ToUpperInvariant(),
            "Existing category",
            "Existing description.",
            DateTimeOffset.UtcNow.AddMinutes(-10),
            Guid.NewGuid());
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
