using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.Modules.Catalog.Domain.UnitsOfMeasure;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.Modules.Catalog.Application.Products.ActivateProduct;

public sealed class ActivateProductHandler(CatalogDbContext dbContext, IClock clock)
{
    public async Task<ActivateProductResult> HandleAsync(
        ActivateProductCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);

        Product? product = await dbContext.Products
            .SingleOrDefaultAsync(existingProduct =>
            existingProduct.Id == command.ProductId &&
            existingProduct.OrganisationId == command.OrganisationId,
            cancellationToken);
        if (product is null)
        {
            throw new NotFoundException(ActivateProductErrors.ProductNotFoundCode, ActivateProductErrors.ProductNotFoundMessage);
        }

        if (product.Status == ProductStatus.Active)
        {
            return new ActivateProductResult(
                ProductId: product.Id,
                Status: product.Status,
                LastModifiedAt: product.LastModifiedAt,
                LastModifiedBy: product.LastModifiedBy,
                RowVersion: product.RowVersion);
        }

        UnitOfMeasure? unitOfMeasure =
            await dbContext.UnitsOfMeasures
            .AsNoTracking()
            .SingleOrDefaultAsync(
                existingUnitOfMeasure =>
                existingUnitOfMeasure.Id == product.UnitOfMeasureId
                , cancellationToken);

        if (unitOfMeasure is null)
        {
            throw new BusinessRuleException(ActivateProductErrors.UnitOfMeasureNotFoundCode, ActivateProductErrors.UnitOfMeasureNotFoundMessage);
        }

        if (unitOfMeasure.Status == UnitOfMeasureStatus.Inactive)
        {

            throw new BusinessRuleException(ActivateProductErrors.UnitOfMeasureInactiveCode, ActivateProductErrors.UnitOfMeasureInactiveMessage);
        }

        if (product.ProductCategoryId.HasValue)
        {
            Guid productCategoryId =
                product.ProductCategoryId.Value;

            ProductCategory? productCategory = await dbContext.ProductCategories
                .AsNoTracking()
                .SingleOrDefaultAsync(existingProductCategory =>
                existingProductCategory.OrganisationId == command.OrganisationId &&
                existingProductCategory.Id == productCategoryId,
                cancellationToken
                );

            if (productCategory is null)
            {
                throw new BusinessRuleException(ActivateProductErrors.ProductCategoryNotFoundCode, ActivateProductErrors.ProductCategoryNotFoundMessage);
            }

            if (productCategory.Status == ProductCategoryStatus.Inactive)
            {
                throw new BusinessRuleException(ActivateProductErrors.ProductCategoryInactiveCode, ActivateProductErrors.ProductCategoryInactiveMessage);
            }
        }
        TaxCategory? taxCategory = await dbContext.TaxCategories
            .AsNoTracking()
            .SingleOrDefaultAsync(existingTaxCategory =>
            existingTaxCategory.Id == product.TaxCategoryId,
            cancellationToken
            );

        if (taxCategory is null)
        {
            throw new BusinessRuleException(ActivateProductErrors.TaxCategoryNotFoundCode, ActivateProductErrors.TaxCategoryNotFoundMessage);
        }

        if (taxCategory.Status == TaxCategoryStatus.Inactive)
        {
            throw new BusinessRuleException(ActivateProductErrors.TaxCategoryInactiveCode, ActivateProductErrors.TaxCategoryInactiveMessage);
        }

        DateTimeOffset modifiedAt = clock.UtcNow;
        DateOnly currentDate = DateOnly.FromDateTime(modifiedAt.UtcDateTime);

        bool isNotEffective =
            currentDate < taxCategory.EffectiveFrom ||
            (taxCategory.EffectiveTo.HasValue && (
            currentDate > taxCategory.EffectiveTo.Value));

        if (isNotEffective)
        {
            throw new BusinessRuleException(ActivateProductErrors.TaxCategoryNotEffectiveCode, ActivateProductErrors.TaxCategoryNotEffectiveMessage);
        }

        var rowVersion = dbContext.Entry(product)
            .Property(existingProduct =>
            existingProduct.RowVersion);

        rowVersion.OriginalValue = command.RowVersion;

        product.Activate(
            modifiedAt: modifiedAt,
            modifiedBy: command.ModifiedBy);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

        }
        catch (DbUpdateConcurrencyException)
        {

            throw new BusinessRuleException(ActivateProductErrors.ConcurrencyConflictCode, ActivateProductErrors.ConcurrencyConflictMessage);
        }

        return new ActivateProductResult(
            ProductId: product.Id,
            Status: product.Status,
            LastModifiedAt: product.LastModifiedAt,
            LastModifiedBy: product.LastModifiedBy,
            RowVersion: product.RowVersion
            );

    }

    private static void ValidateCommand(ActivateProductCommand command)
    {
        if (command.OrganisationId == Guid.Empty)
        {
            throw new RequestValidationException(ActivateProductErrors.OrganisationRequiredCode, ActivateProductErrors.OrganisationRequiredMessage);
        }

        if (command.ProductId == Guid.Empty)
        {
            throw new RequestValidationException(ActivateProductErrors.ProductIdRequiredCode, ActivateProductErrors.ProductIdRequiredMessage);
        }

        if (command.ModifiedBy == Guid.Empty)
        {
            throw new RequestValidationException(ActivateProductErrors.ModifiedByRequiredCode, ActivateProductErrors.ModifiedByRequiredMessage);
        }

        if (command.RowVersion is null || command.RowVersion.Length == 0)
        {
            throw new RequestValidationException(ActivateProductErrors.RowVersionRequiredCode, ActivateProductErrors.RowVersionRequiredMessage);
        }

    }
}


