using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.Modules.Catalog.Domain.UnitsOfMeasure;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.Modules.Catalog.Application.Products.GetProductReferenceData;

public sealed class GetProductReferenceDataHandler(
    CatalogDbContext dbContext,
    IClock clock)
{
    public async Task<GetProductReferenceDataResult> HandleAsync(
        GetProductReferenceDataQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.OrganisationId == Guid.Empty)
        {
            throw new RequestValidationException(
                GetProductReferenceDataErrors.OrganisationRequiredCode,
                GetProductReferenceDataErrors.OrganisationRequiredMessage);
        }

        DateOnly currentDate =
            DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);

        List<ProductReferenceDataItem> unitsOfMeasure =
            await dbContext.UnitsOfMeasures
                .AsNoTracking()
                .Where(unitOfMeasure =>
                    unitOfMeasure.Status == UnitOfMeasureStatus.Active)
                .OrderBy(unitOfMeasure => unitOfMeasure.Code)
                .ThenBy(unitOfMeasure => unitOfMeasure.Id)
                .Select(unitOfMeasure =>
                    new ProductReferenceDataItem(
                        unitOfMeasure.Id,
                        unitOfMeasure.Code,
                        unitOfMeasure.Name))
                .ToListAsync(cancellationToken);

        List<ProductReferenceDataItem> productCategories =
            await dbContext.ProductCategories
                .AsNoTracking()
                .Where(productCategory =>
                    productCategory.OrganisationId == query.OrganisationId
                    && productCategory.Status == ProductCategoryStatus.Active)
                .OrderBy(productCategory => productCategory.Code)
                .ThenBy(productCategory => productCategory.Id)
                .Select(productCategory =>
                    new ProductReferenceDataItem(
                        productCategory.Id,
                        productCategory.Code,
                        productCategory.Name))
                .ToListAsync(cancellationToken);

        List<ProductReferenceDataItem> taxCategories =
            await dbContext.TaxCategories
                .AsNoTracking()
                .Where(taxCategory =>
                    taxCategory.Status == TaxCategoryStatus.Active
                    && taxCategory.EffectiveFrom <= currentDate
                    && (!taxCategory.EffectiveTo.HasValue
                        || taxCategory.EffectiveTo.Value >= currentDate))
                .OrderBy(taxCategory => taxCategory.Code)
                .ThenBy(taxCategory => taxCategory.Id)
                .Select(taxCategory =>
                    new ProductReferenceDataItem(
                        taxCategory.Id,
                        taxCategory.Code,
                        taxCategory.Name))
                .ToListAsync(cancellationToken);

        return new GetProductReferenceDataResult(
            UnitsOfMeasure: unitsOfMeasure,
            ProductCategories: productCategories,
            TaxCategories: taxCategories);
    }
}
