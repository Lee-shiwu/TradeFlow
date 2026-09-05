using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Application.Products.GetProductReferenceData;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Catalog.Products;

public sealed class GetProductReferenceDataHandlerTests(
    ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task HandleAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        GetProductReferenceDataHandler handler =
            scope.ServiceProvider
                .GetRequiredService<GetProductReferenceDataHandler>();

        ArgumentNullException exception =
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => handler.HandleAsync(
                    null!,
                    CancellationToken.None));

        Assert.Equal("query", exception.ParamName);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyOrganisationId_ThrowsRequestValidationException()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        GetProductReferenceDataHandler handler =
            scope.ServiceProvider
                .GetRequiredService<GetProductReferenceDataHandler>();

        GetProductReferenceDataQuery query =
            new(Guid.Empty);

        RequestValidationException exception =
            await Assert.ThrowsAsync<RequestValidationException>(
                () => handler.HandleAsync(
                    query,
                    CancellationToken.None));

        Assert.Equal(
            "GET_PRODUCT_REFERENCE_DATA_ORGANISATION_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Organisation is required.",
            exception.Message);
    }

    [Fact]
    public async Task HandleAsync_ReturnsActiveReferenceDataForRequestedOrganisation()
    {
        using IServiceScope scope =
            factory.Services.CreateScope();

        CatalogDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

        await dbContext.Database.MigrateAsync();

        GetProductReferenceDataHandler handler =
            scope.ServiceProvider
                .GetRequiredService<GetProductReferenceDataHandler>();

        Guid organisationId = Guid.NewGuid();
        Guid otherOrganisationId = Guid.NewGuid();
        Guid createdBy = Guid.NewGuid();
        Guid modifiedBy = Guid.NewGuid();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow.AddMinutes(-2);

        ProductCategory requestedCategory =
            CreateProductCategory(
                organisationId,
                "Requested category",
                createdAt,
                createdBy);

        ProductCategory inactiveCategory =
            CreateProductCategory(
                organisationId,
                "Inactive category",
                createdAt,
                createdBy);

        inactiveCategory.Deactivate(
            DateTimeOffset.UtcNow.AddMinutes(-1),
            modifiedBy);

        ProductCategory otherCategory =
            CreateProductCategory(
                otherOrganisationId,
                "Other category",
                createdAt,
                createdBy);

        dbContext.ProductCategories.AddRange(
            requestedCategory,
            inactiveCategory,
            otherCategory);

        await dbContext.SaveChangesAsync();

        try
        {
            dbContext.ChangeTracker.Clear();

            GetProductReferenceDataResult result =
                await handler.HandleAsync(
                    new GetProductReferenceDataQuery(organisationId),
                    CancellationToken.None);

            Assert.NotEmpty(result.UnitsOfMeasure);
            Assert.NotEmpty(result.TaxCategories);

            Assert.Contains(
                result.ProductCategories,
                item => item.Id == requestedCategory.Id);

            Assert.DoesNotContain(
                result.ProductCategories,
                item => item.Id == inactiveCategory.Id);

            Assert.DoesNotContain(
                result.ProductCategories,
                item => item.Id == otherCategory.Id);

            Assert.Equal(
                result.UnitsOfMeasure
                    .OrderBy(item => item.Code)
                    .ThenBy(item => item.Id),
                result.UnitsOfMeasure);

            Assert.Equal(
                result.ProductCategories
                    .OrderBy(item => item.Code)
                    .ThenBy(item => item.Id),
                result.ProductCategories);

            Assert.Equal(
                result.TaxCategories
                    .OrderBy(item => item.Code)
                    .ThenBy(item => item.Id),
                result.TaxCategories);

            Assert.Empty(dbContext.ChangeTracker.Entries());
        }
        finally
        {
            await DeleteProductCategoriesAsync(
                dbContext,
                requestedCategory.Id,
                inactiveCategory.Id,
                otherCategory.Id);
        }
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
