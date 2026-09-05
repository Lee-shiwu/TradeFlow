using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Api.Endpoints.Catalog.Products;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.Products;

public sealed class GetProductReferenceDataEndpointTests(
    ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Get_WithValidIdentity_ReturnsReferenceDataForOrganisation()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        Guid organisationId = Guid.NewGuid();
        Guid otherOrganisationId = Guid.NewGuid();
        Guid createdBy = Guid.NewGuid();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;

        ProductCategory requestedCategory =
            CreateProductCategory(
                organisationId,
                "Requested category",
                createdAt,
                createdBy);

        ProductCategory otherCategory =
            CreateProductCategory(
                otherOrganisationId,
                "Other category",
                createdAt,
                createdBy);

        dbContext.ProductCategories.AddRange(
            requestedCategory,
            otherCategory);

        await dbContext.SaveChangesAsync();

        using HttpClient client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            organisationId.ToString());

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            Guid.NewGuid().ToString());

        try
        {
            HttpResponseMessage response =
                await client.GetAsync(
                    "/api/v1/catalog/products/reference-data");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Equal(
                "application/json",
                response.Content.Headers.ContentType?.MediaType);

            GetProductReferenceDataResponse? responseBody =
                await response.Content
                    .ReadFromJsonAsync<GetProductReferenceDataResponse>();

            Assert.NotNull(responseBody);
            Assert.NotEmpty(responseBody.UnitsOfMeasure);
            Assert.NotEmpty(responseBody.TaxCategories);

            Assert.Contains(
                responseBody.ProductCategories,
                item => item.Id == requestedCategory.Id);

            Assert.DoesNotContain(
                responseBody.ProductCategories,
                item => item.Id == otherCategory.Id);
        }
        finally
        {
            await DeleteProductCategoriesAsync(
                dbContext,
                requestedCategory.Id,
                otherCategory.Id);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Get_WithMissingOrInvalidOrganisationHeader_ReturnsBadRequest(
        string? organisationHeader)
    {
        using HttpClient client = factory.CreateClient();

        if (organisationHeader is not null)
        {
            client.DefaultRequestHeaders.Add(
                "X-Organisation-Id",
                organisationHeader);
        }

        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            Guid.NewGuid().ToString());

        HttpResponseMessage response =
            await client.GetAsync(
                "/api/v1/catalog/products/reference-data");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        ValidationProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("X-Organisation-Id"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Get_WithMissingOrInvalidUserHeader_ReturnsBadRequest(
        string? userHeader)
    {
        using HttpClient client = factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            Guid.NewGuid().ToString());

        if (userHeader is not null)
        {
            client.DefaultRequestHeaders.Add(
                "X-User-Id",
                userHeader);
        }

        HttpResponseMessage response =
            await client.GetAsync(
                "/api/v1/catalog/products/reference-data");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        ValidationProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("X-User-Id"));
    }

    private static ProductCategory CreateProductCategory(
        Guid organisationId,
        string name,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        return ProductCategory.Create(
            organisationId,
            $"CATEGORY-{Guid.NewGuid():N}",
            name,
            null,
            createdAt,
            createdBy);
    }

    private static async Task DeleteProductCategoriesAsync(
        CatalogDbContext dbContext,
        params Guid[] productCategoryIds)
    {
        await dbContext.ProductCategories
            .Where(productCategory =>
                productCategoryIds.Contains(productCategory.Id))
            .ExecuteDeleteAsync();
    }
}
