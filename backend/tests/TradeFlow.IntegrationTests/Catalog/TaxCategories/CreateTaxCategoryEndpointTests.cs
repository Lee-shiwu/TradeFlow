using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Api.Endpoints.Catalog.TaxCategories;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.TaxCategories;

public sealed class CreateTaxCategoryEndpointTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Post_WithValidRequest_CreatesTaxCategory()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await dbContext.Database.MigrateAsync();
        using HttpClient client = CreateAuthenticatedClient();

        string suffix = CreateSuffix();
        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                "/api/v1/catalog/tax-categories",
                new CreateTaxCategoryRequest(
                    $" api-{suffix} ",
                    "  Regional   levy  ",
                    "  Regional standard tax.  ",
                    0.125m,
                    TaxTreatment.StandardRated,
                    new DateOnly(2026, 10, 1),
                    null));

        CreateTaxCategoryResponse? body = null;

        try
        {
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            body =
                await response.Content
                    .ReadFromJsonAsync<CreateTaxCategoryResponse>();
            Assert.NotNull(body);
            Assert.Equal($"API-{suffix}", body.Code);
            Assert.Equal("Regional levy", body.Name);
            Assert.Equal("Regional standard tax.", body.Description);
            Assert.Equal(0.125m, body.Rate);
            Assert.Equal(TaxTreatment.StandardRated, body.Treatment);
            Assert.Equal(TaxCategoryStatus.Active, body.Status);
            Assert.False(string.IsNullOrWhiteSpace(body.RowVersion));
            Assert.Equal(
                $"/api/v1/catalog/tax-categories/{body.TaxCategoryId}",
                response.Headers.Location?.ToString());

            TaxCategory? persisted =
                await dbContext.TaxCategories
                    .AsNoTracking()
                    .SingleOrDefaultAsync(category =>
                        category.Id == body.TaxCategoryId);
            Assert.NotNull(persisted);
        }
        finally
        {
            if (body is not null)
            {
                await DeleteCategoriesAsync(dbContext, body.TaxCategoryId);
            }
        }
    }

    [Fact]
    public async Task Post_WithDuplicateCode_ReturnsConflictProblem()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await dbContext.Database.MigrateAsync();

        TaxCategory existing =
            TaxCategory.Create(
                $"DUP-{CreateSuffix()}",
                "Existing tax category",
                null,
                0m,
                TaxTreatment.ZeroRated,
                new DateOnly(2026, 1, 1),
                null);
        dbContext.TaxCategories.Add(existing);
        await dbContext.SaveChangesAsync();
        using HttpClient client = CreateAuthenticatedClient();

        try
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    "/api/v1/catalog/tax-categories",
                    new CreateTaxCategoryRequest(
                        existing.Code.ToLowerInvariant(),
                        "Duplicate tax category",
                        null,
                        0m,
                        TaxTreatment.ZeroRated,
                        new DateOnly(2026, 1, 1),
                        null));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            ProblemDetails? problem =
                await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(problem);
            Assert.Equal(
                "TAX_CATEGORY_CODE_ALREADY_EXISTS",
                problem.Extensions["code"]?.ToString());
        }
        finally
        {
            await DeleteCategoriesAsync(dbContext, existing.Id);
        }
    }

    [Fact]
    public async Task Post_WithTreatmentRateMismatch_ReturnsBusinessRuleProblem()
    {
        using HttpClient client = CreateAuthenticatedClient();

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                "/api/v1/catalog/tax-categories",
                new CreateTaxCategoryRequest(
                    $"INVALID-{CreateSuffix()}",
                    "Invalid zero-rated tax",
                    null,
                    0.15m,
                    TaxTreatment.ZeroRated,
                    new DateOnly(2026, 1, 1),
                    null));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemDetails? problem =
            await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(
            "TAX_CATEGORY_TREATMENT_RATE_MISMATCH",
            problem.Extensions["code"]?.ToString());
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
                "/api/v1/catalog/tax-categories",
                ValidRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(missingHeader, problem.Errors.Keys);
    }

    private static CreateTaxCategoryRequest ValidRequest()
    {
        return new CreateTaxCategoryRequest(
            $"CREATE-{CreateSuffix()}",
            "Regional levy",
            null,
            0.125m,
            TaxTreatment.StandardRated,
            new DateOnly(2026, 10, 1),
            null);
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

    private static string CreateSuffix()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }

    private static async Task DeleteCategoriesAsync(
        CatalogDbContext dbContext,
        params Guid[] taxCategoryIds)
    {
        dbContext.ChangeTracker.Clear();
        await dbContext.TaxCategories
            .Where(category => taxCategoryIds.Contains(category.Id))
            .ExecuteDeleteAsync();
    }
}
