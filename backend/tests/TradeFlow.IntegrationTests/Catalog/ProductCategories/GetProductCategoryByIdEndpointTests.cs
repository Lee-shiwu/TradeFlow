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

public sealed class GetProductCategoryByIdEndpointTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Get_WithExistingCategory_ReturnsCategoryDetails()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        ProductCategory category = CreateCategory(organisationId);

        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();

        using HttpClient client = CreateAuthenticatedClient(organisationId);

        try
        {
            HttpResponseMessage response =
                await client.GetAsync(
                    $"/api/v1/catalog/product-categories/{category.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string json = await response.Content.ReadAsStringAsync();
            using JsonDocument document = JsonDocument.Parse(json);

            Assert.Equal(
                "Active",
                document.RootElement.GetProperty("status").GetString());

            GetProductCategoryByIdResponse? body =
                await response.Content
                    .ReadFromJsonAsync<GetProductCategoryByIdResponse>();

            Assert.NotNull(body);
            Assert.Equal(category.Id, body.ProductCategoryId);
            Assert.Equal(category.Code, body.Code);
            Assert.Equal(category.Name, body.Name);
            Assert.Equal(category.Description, body.Description);
            Assert.Equal(category.Status, body.Status);
            Assert.Equal(category.CreatedAt, body.CreatedAt);
            Assert.Equal(category.CreatedBy, body.CreatedBy);
            Assert.Equal(category.LastModifiedAt, body.LastModifiedAt);
            Assert.Equal(category.LastModifiedBy, body.LastModifiedBy);
            Assert.Equal(
                Convert.ToBase64String(category.RowVersion),
                body.RowVersion);
        }
        finally
        {
            await DeleteCategoryAsync(dbContext, category.Id);
        }
    }

    [Fact]
    public async Task Get_WithUnknownCategory_ReturnsNotFoundProblem()
    {
        using HttpClient client =
            CreateAuthenticatedClient(Guid.NewGuid());

        HttpResponseMessage response =
            await client.GetAsync(
                $"/api/v1/catalog/product-categories/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        ProblemDetails? problem =
            await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal("Resource not found", problem.Title);

        JsonElement code =
            Assert.IsType<JsonElement>(problem.Extensions["code"]);

        Assert.Equal("PRODUCT_CATEGORY_NOT_FOUND", code.GetString());
    }

    [Fact]
    public async Task Get_WithCategoryFromAnotherOrganisation_ReturnsNotFound()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        ProductCategory category = CreateCategory(Guid.NewGuid());
        dbContext.ProductCategories.Add(category);
        await dbContext.SaveChangesAsync();

        using HttpClient client = CreateAuthenticatedClient(Guid.NewGuid());

        try
        {
            HttpResponseMessage response =
                await client.GetAsync(
                    $"/api/v1/catalog/product-categories/{category.Id}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        finally
        {
            await DeleteCategoryAsync(dbContext, category.Id);
        }
    }

    [Theory]
    [InlineData("X-Organisation-Id")]
    [InlineData("X-User-Id")]
    public async Task Get_WithMissingIdentityHeader_ReturnsBadRequest(
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
            await client.GetAsync(
                $"/api/v1/catalog/product-categories/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        ValidationProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Contains(missingHeader, problem.Errors.Keys);
    }

    private HttpClient CreateAuthenticatedClient(Guid organisationId)
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            organisationId.ToString());
        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            Guid.NewGuid().ToString());
        return client;
    }

    private static ProductCategory CreateCategory(Guid organisationId)
    {
        return ProductCategory.Create(
            organisationId,
            $"DETAIL-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            "Office products",
            "Products used in the office.",
            DateTimeOffset.UtcNow.AddMinutes(-5),
            Guid.NewGuid());
    }

    private static async Task DeleteCategoryAsync(
        CatalogDbContext dbContext,
        Guid productCategoryId)
    {
        await dbContext.ProductCategories
            .Where(category => category.Id == productCategoryId)
            .ExecuteDeleteAsync();
    }
}
