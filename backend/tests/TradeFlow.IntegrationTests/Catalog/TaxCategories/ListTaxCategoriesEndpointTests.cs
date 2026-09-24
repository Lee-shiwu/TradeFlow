using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Api.Endpoints.Catalog.TaxCategories;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.TaxCategories;

public sealed class ListTaxCategoriesEndpointTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Get_WithSearch_ReturnsTaxInformation()
    {
        await EnsureDatabaseAsync();
        using HttpClient client = CreateAuthenticatedClient();

        HttpResponseMessage response = await client.GetAsync(
            "/api/v1/catalog/tax-categories?search=GST15&pageNumber=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ListTaxCategoriesResponse? body =
            await response.Content.ReadFromJsonAsync<ListTaxCategoriesResponse>();
        Assert.NotNull(body);
        ListTaxCategoryItemResponse item = Assert.Single(body.Items);
        Assert.Equal("GST15", item.Code);
        Assert.Equal(0.1500m, item.Rate);
        Assert.Equal(TaxTreatment.StandardRated, item.Treatment);
        Assert.Equal(TaxCategoryStatus.Active, item.Status);
        Assert.Equal(1, body.PageNumber);
        Assert.Equal(20, body.PageSize);
        Assert.Equal(1, body.TotalCount);
        Assert.Equal(1, body.TotalPages);
    }

    [Fact]
    public async Task Get_WithInvalidPagination_ReturnsValidationProblem()
    {
        using HttpClient client = CreateAuthenticatedClient();

        HttpResponseMessage response = await client.GetAsync(
            "/api/v1/catalog/tax-categories?pageNumber=0&pageSize=20");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ProblemDetails? problem =
            await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("LIST_TAX_CATEGORIES_PAGE_NUMBER_INVALID",
            problem.Extensions["code"]?.ToString());
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
            await client.GetAsync("/api/v1/catalog/tax-categories");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(missingHeader, problem.Errors.Keys);
    }

    private async Task EnsureDatabaseAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    private HttpClient CreateAuthenticatedClient()
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            Guid.NewGuid().ToString());
        return client;
    }
}
