using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Api.Endpoints.Catalog.TaxCategories;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.TaxCategories;

public sealed class GetTaxCategoryByIdEndpointTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private static readonly Guid StandardGstId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task Get_WithExistingCategory_ReturnsDetails()
    {
        await EnsureDatabaseAsync();
        using HttpClient client = CreateAuthenticatedClient();

        HttpResponseMessage response =
            await client.GetAsync(
                $"/api/v1/catalog/tax-categories/{StandardGstId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string json = await response.Content.ReadAsStringAsync();
        using JsonDocument document = JsonDocument.Parse(json);
        Assert.Equal(
            "StandardRated",
            document.RootElement.GetProperty("treatment").GetString());
        Assert.Equal(
            "Active",
            document.RootElement.GetProperty("status").GetString());

        GetTaxCategoryByIdResponse? body =
            await response.Content.ReadFromJsonAsync<GetTaxCategoryByIdResponse>();

        Assert.NotNull(body);
        Assert.Equal(StandardGstId, body.TaxCategoryId);
        Assert.Equal("GST15", body.Code);
        Assert.Equal("Standard GST", body.Name);
        Assert.Equal("Standard-rated supplies at 15% GST.", body.Description);
        Assert.Equal(0.1500m, body.Rate);
        Assert.Equal(TaxTreatment.StandardRated, body.Treatment);
        Assert.Equal(TaxCategoryStatus.Active, body.Status);
        Assert.Equal(new DateOnly(2010, 10, 1), body.EffectiveFrom);
        Assert.Null(body.EffectiveTo);
        Assert.NotEmpty(Convert.FromBase64String(body.RowVersion));
    }

    [Fact]
    public async Task Get_WithUnknownCategory_ReturnsNotFoundProblem()
    {
        using HttpClient client = CreateAuthenticatedClient();

        HttpResponseMessage response =
            await client.GetAsync(
                $"/api/v1/catalog/tax-categories/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        ProblemDetails? problem =
            await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Resource not found", problem.Title);
        Assert.Equal(
            "TAX_CATEGORY_NOT_FOUND",
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
            await client.GetAsync(
                $"/api/v1/catalog/tax-categories/{StandardGstId}");

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
