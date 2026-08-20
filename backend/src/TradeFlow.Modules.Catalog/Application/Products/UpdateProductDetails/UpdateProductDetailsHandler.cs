using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.Modules.Catalog.Application.Products.UpdateProductDetails;

public sealed class UpdateProductDetailsHandler(CatalogDbContext dbContext, IClock clock)
{
    public async Task<UpdateProductDetailsResult> HandleAsync(
        UpdateProductDetailsCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);

        Product? product = await dbContext.Products
            .SingleOrDefaultAsync(existingProduct =>
                existingProduct.OrganisationId == command.OrganisationId &&
                existingProduct.Id == command.ProductId,
            cancellationToken);

        if (product is null)
        {
            throw new NotFoundException(UpdateProductDetailsErrors.ProductNotFoundCode, UpdateProductDetailsErrors.ProductNotFoundMessage);
        }

        if (command.ProductCategoryId.HasValue)
        {
            ProductCategory? productCategory = await dbContext.ProductCategories
                .AsNoTracking()
                .SingleOrDefaultAsync(existingProductCategory =>
            existingProductCategory.Id == command.ProductCategoryId.Value &&
            existingProductCategory.OrganisationId == command.OrganisationId,
            cancellationToken);


            if (productCategory is null)
            {
                throw new BusinessRuleException(UpdateProductDetailsErrors.ProductCategoryNotFoundCode, UpdateProductDetailsErrors.ProductCategoryNotFoundMessage);
            }

            if (productCategory.Status == ProductCategoryStatus.Inactive)
            {
                throw new BusinessRuleException(UpdateProductDetailsErrors.ProductCategoryInactiveCode, UpdateProductDetailsErrors.ProductCategoryInactiveMessage);
            }
        }

        TaxCategory? taxCategory = await dbContext.TaxCategories
            .AsNoTracking()
            .SingleOrDefaultAsync(existingTaxCategory =>
            existingTaxCategory.Id == command.TaxCategoryId,
             cancellationToken);

        if (taxCategory is null)
        {

            throw new BusinessRuleException(UpdateProductDetailsErrors.TaxCategoryNotFoundCode, UpdateProductDetailsErrors.TaxCategoryNotFoundMessage);
        }

        if (taxCategory.Status == TaxCategoryStatus.Inactive)
        {
            throw new BusinessRuleException(UpdateProductDetailsErrors.TaxCategoryInactiveCode, UpdateProductDetailsErrors.TaxCategoryInactiveMessage);
        }

        DateTimeOffset modifiedAt = clock.UtcNow;
        DateOnly currentDate = DateOnly.FromDateTime(modifiedAt.UtcDateTime);

        bool isNotEffective =
            currentDate < taxCategory.EffectiveFrom ||
            (taxCategory.EffectiveTo.HasValue &&
            currentDate > taxCategory.EffectiveTo.Value);

        if (isNotEffective)
        {
            throw new BusinessRuleException(UpdateProductDetailsErrors.TaxCategoryNotEffectiveCode, UpdateProductDetailsErrors.TaxCategoryNotEffectiveMessage);
        }

        var rowVersionEntry = dbContext.Entry(product)
            .Property(existingProduct =>
                existingProduct.RowVersion);

        rowVersionEntry.OriginalValue = command.RowVersion;

        product.UpdateDetails(
             name: command.Name,
             description: command.Description,
             productCategoryId: command.ProductCategoryId,
             taxCategoryId: command.TaxCategoryId,
             modifiedAt: modifiedAt,
             modifiedBy: command.ModifiedBy);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {

            throw new BusinessRuleException(UpdateProductDetailsErrors.ConcurrencyConflictCode, UpdateProductDetailsErrors.ConcurrencyConflictMessage);
        }

        return new UpdateProductDetailsResult(
            ProductId: product.Id,
            Name: product.Name,
            Description: product.Description,
            ProductCategoryId: product.ProductCategoryId,
            TaxCategoryId: product.TaxCategoryId,
            LastModifiedAt: product.LastModifiedAt,
            LastModifiedBy: product.LastModifiedBy,
            RowVersion: product.RowVersion
            );
    }


    private static void ValidateCommand(UpdateProductDetailsCommand command)
    {
        if (command.OrganisationId == Guid.Empty)
        {
            throw new RequestValidationException(UpdateProductDetailsErrors.OrganisationRequiredCode, UpdateProductDetailsErrors.OrganisationRequiredMessage);
        }
        if (command.ProductId == Guid.Empty)
        {
            throw new RequestValidationException(UpdateProductDetailsErrors.ProductIdRequiredCode, UpdateProductDetailsErrors.ProductIdRequiredMessage);
        }
        if (command.ModifiedBy == Guid.Empty)
        {
            throw new RequestValidationException(UpdateProductDetailsErrors.ModifiedByRequiredCode, UpdateProductDetailsErrors.ModifiedByRequiredMessage);
        }

        if (command.TaxCategoryId == Guid.Empty)
        {
            throw new RequestValidationException(UpdateProductDetailsErrors.TaxCategoryRequiredCode, UpdateProductDetailsErrors.TaxCategoryRequiredMessage);
        }

        if (command.RowVersion == null || command.RowVersion.Length == 0)
        {
            throw new RequestValidationException(UpdateProductDetailsErrors.RowVersionRequiredCode, UpdateProductDetailsErrors.RowVersionRequiredMessage);

        }

    }

}
