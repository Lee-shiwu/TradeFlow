using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Application.TaxCategories.ListTaxCategories;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.TaxCategories;

public sealed class ListTaxCategoriesHandlerTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task HandleAsync_WithCodeSearch_ReturnsMatchingTaxCategory()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await dbContext.Database.MigrateAsync();
        ListTaxCategoriesHandler handler = new(dbContext);

        ListTaxCategoriesResult result = await handler.HandleAsync(
            new ListTaxCategoriesQuery(" GST15 ", null, 1, 20),
            CancellationToken.None);

        ListTaxCategoryItem item = Assert.Single(result.Items);
        Assert.Equal("GST15", item.Code);
        Assert.Equal("Standard GST", item.Name);
        Assert.Equal(0.1500m, item.Rate);
        Assert.Equal(TaxTreatment.StandardRated, item.Treatment);
        Assert.Equal(TaxCategoryStatus.Active, item.Status);
        Assert.Equal(new DateOnly(2010, 10, 1), item.EffectiveFrom);
        Assert.Null(item.EffectiveTo);
    }

    [Fact]
    public async Task HandleAsync_WithPagination_ReturnsStablePages()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await dbContext.Database.MigrateAsync();
        ListTaxCategoriesHandler handler = new(dbContext);

        ListTaxCategoriesResult firstPage = await handler.HandleAsync(
            new ListTaxCategoriesQuery(null, TaxCategoryStatus.Active, 1, 2),
            CancellationToken.None);
        ListTaxCategoriesResult secondPage = await handler.HandleAsync(
            new ListTaxCategoriesQuery(null, TaxCategoryStatus.Active, 2, 2),
            CancellationToken.None);

        Assert.Equal(2, firstPage.Items.Count);
        Assert.NotEmpty(secondPage.Items);
        Assert.True(firstPage.TotalCount >= 3);
        Assert.Equal(
            ((firstPage.TotalCount - 1) / 2) + 1,
            firstPage.TotalPages);
        Assert.Empty(
            firstPage.Items.Select(item => item.TaxCategoryId)
                .Intersect(secondPage.Items.Select(item => item.TaxCategoryId)));
        Assert.All(firstPage.Items, item =>
            Assert.Equal(TaxCategoryStatus.Active, item.Status));
    }

    [Theory]
    [InlineData(0, 20, "LIST_TAX_CATEGORIES_PAGE_NUMBER_INVALID")]
    [InlineData(1, 0, "LIST_TAX_CATEGORIES_PAGE_SIZE_INVALID")]
    [InlineData(1, 101, "LIST_TAX_CATEGORIES_PAGE_SIZE_INVALID")]
    public async Task HandleAsync_WithInvalidPagination_ThrowsValidation(
        int pageNumber,
        int pageSize,
        string expectedCode)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        ListTaxCategoriesHandler handler = new(dbContext);

        RequestValidationException exception =
            await Assert.ThrowsAsync<RequestValidationException>(() =>
                handler.HandleAsync(
                    new ListTaxCategoriesQuery(
                        null,
                        null,
                        pageNumber,
                        pageSize),
                    CancellationToken.None));

        Assert.Equal(expectedCode, exception.Code);
    }

    [Fact]
    public async Task HandleAsync_WithLongSearch_ThrowsValidation()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        ListTaxCategoriesHandler handler = new(dbContext);

        RequestValidationException exception =
            await Assert.ThrowsAsync<RequestValidationException>(() =>
                handler.HandleAsync(
                    new ListTaxCategoriesQuery(new string('x', 101), null, 1, 20),
                    CancellationToken.None));

        Assert.Equal("LIST_TAX_CATEGORIES_SEARCH_INVALID", exception.Code);
    }
}
