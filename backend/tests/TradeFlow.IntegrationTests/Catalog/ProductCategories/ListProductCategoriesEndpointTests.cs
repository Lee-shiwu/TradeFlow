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

public sealed class ListProductCategoriesEndpointTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Get_ReturnsOnlyCurrentOrganisationCategories()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid otherOrganisationId = Guid.NewGuid();
        string suffix = CreateSuffix();

        ProductCategory categoryB =
            CreateCategory(organisationId, $"LIST-{suffix}-B", "Category B");
        ProductCategory categoryA =
            CreateCategory(organisationId, $"LIST-{suffix}-A", "Category A");
        ProductCategory other =
            CreateCategory(otherOrganisationId, $"LIST-{suffix}-OTHER", "Other");

        dbContext.ProductCategories.AddRange(categoryB, categoryA, other);
        await dbContext.SaveChangesAsync();

        using HttpClient client = CreateAuthenticatedClient(organisationId);

        try
        {
            HttpResponseMessage response =
                await client.GetAsync("/api/v1/catalog/product-categories");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string json = await response.Content.ReadAsStringAsync();
            using JsonDocument document = JsonDocument.Parse(json);

            Assert.Equal(
                "Active",
                document.RootElement
                    .GetProperty("items")[0]
                    .GetProperty("status")
                    .GetString());

            ListProductCategoriesResponse? body =
                await response.Content
                    .ReadFromJsonAsync<ListProductCategoriesResponse>();

            Assert.NotNull(body);
            Assert.Equal(2, body.TotalCount);
            Assert.Equal(1, body.TotalPages);
            Assert.Equal(1, body.PageNumber);
            Assert.Equal(20, body.PageSize);
            Assert.Equal(categoryA.Id, body.Items[0].ProductCategoryId);
            Assert.Equal(categoryB.Id, body.Items[1].ProductCategoryId);
            Assert.DoesNotContain(
                body.Items,
                item => item.ProductCategoryId == other.Id);
        }
        finally
        {
            await DeleteCategoriesAsync(
                dbContext,
                organisationId,
                otherOrganisationId);
        }
    }

    [Fact]
    public async Task Get_BindsSearchStatusAndPagination()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        string suffix = CreateSuffix();

        ProductCategory activeA =
            CreateCategory(organisationId, $"FILTER-{suffix}-A", "Office A");
        ProductCategory activeB =
            CreateCategory(organisationId, $"FILTER-{suffix}-B", "Office B");
        ProductCategory inactive =
            CreateCategory(organisationId, $"FILTER-{suffix}-C", "Office C");

        inactive.Deactivate(DateTimeOffset.UtcNow, Guid.NewGuid());

        dbContext.ProductCategories.AddRange(activeA, activeB, inactive);
        await dbContext.SaveChangesAsync();

        using HttpClient client = CreateAuthenticatedClient(organisationId);

        try
        {
            string url =
                "/api/v1/catalog/product-categories"
                + $"?search={suffix}"
                + "&status=Active"
                + "&pageNumber=2"
                + "&pageSize=1";

            ListProductCategoriesResponse? body =
                await client.GetFromJsonAsync<ListProductCategoriesResponse>(url);

            Assert.NotNull(body);
            Assert.Equal(2, body.TotalCount);
            Assert.Equal(2, body.TotalPages);
            Assert.Equal(2, body.PageNumber);
            Assert.Equal(activeB.Id, Assert.Single(body.Items).ProductCategoryId);
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, organisationId);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Get_WithInvalidOrganisationHeader_ReturnsBadRequest(
        string? headerValue)
    {
        using HttpClient client = factory.CreateClient();

        if (headerValue is not null)
        {
            client.DefaultRequestHeaders.Add("X-Organisation-Id", headerValue);
        }

        client.DefaultRequestHeaders.Add("X-User-Id", Guid.NewGuid().ToString());

        HttpResponseMessage response =
            await client.GetAsync("/api/v1/catalog/product-categories");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        ValidationProblemDetails? problem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Contains("X-Organisation-Id", problem.Errors.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Get_WithInvalidUserHeader_ReturnsBadRequest(
        string? headerValue)
    {
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            Guid.NewGuid().ToString());

        if (headerValue is not null)
        {
            client.DefaultRequestHeaders.Add("X-User-Id", headerValue);
        }

        HttpResponseMessage response =
            await client.GetAsync("/api/v1/catalog/product-categories");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        ValidationProblemDetails? problem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Contains("X-User-Id", problem.Errors.Keys);
    }

    [Fact]
    public async Task Get_WithInvalidPagination_ReturnsRequestProblem()
    {
        using HttpClient client = CreateAuthenticatedClient(Guid.NewGuid());

        HttpResponseMessage response =
            await client.GetAsync(
                "/api/v1/catalog/product-categories?pageNumber=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        ProblemDetails? problem =
            await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal("Request validation failed", problem.Title);
        Assert.Equal(
            "Page number must be greater than or equal to 1.",
            problem.Detail);

        JsonElement code =
            Assert.IsType<JsonElement>(problem.Extensions["code"]);

        Assert.Equal(
            "LIST_PRODUCT_CATEGORIES_PAGE_NUMBER_INVALID",
            code.GetString());
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

    private static ProductCategory CreateCategory(
        Guid organisationId,
        string code,
        string name)
    {
        return ProductCategory.Create(
            organisationId: organisationId,
            code: code,
            name: name,
            description: $"Description for {name}",
            createdAt: DateTimeOffset.UtcNow.AddMinutes(-5),
            createdBy: Guid.NewGuid());
    }

    private static string CreateSuffix()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }

    private static async Task DeleteCategoriesAsync(
        CatalogDbContext dbContext,
        params Guid[] organisationIds)
    {
        await dbContext.ProductCategories
            .Where(category =>
                organisationIds.Contains(category.OrganisationId))
            .ExecuteDeleteAsync();
    }
}
