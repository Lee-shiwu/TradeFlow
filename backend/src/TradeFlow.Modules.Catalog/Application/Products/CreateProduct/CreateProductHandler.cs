using Microsoft.EntityFrameworkCore;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog.Domain.UnitsOfMeasure;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using Microsoft.Data.SqlClient;

namespace TradeFlow.Modules.Catalog.Application.Products.CreateProduct;

public sealed class CreateProductHandler(CatalogDbContext dbContext, IClock clock)
{
    private const string SkuUniqueIndexName =
            "UX_Products_OrganisationId_Sku";
    public async Task<CreateProductResult> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        DateTimeOffset createdAt = clock.UtcNow;
        Product product = Product.Create(
            organisationId: command.OrganisationId,
            sku: command.Sku,
            name: command.Name,
            description: command.Description,
            unitOfMeasureId: command.UnitOfMeasureId,
            productCategoryId: command.ProductCategoryId,
            taxCategoryId: command.TaxCategoryId,
            createdAt: createdAt,
            createdBy: command.CreatedBy
            );
        bool skuAlreadyExists =
            await dbContext.Products.AnyAsync(
                existingProduct =>
                existingProduct.OrganisationId == product.OrganisationId &&
                existingProduct.Sku == product.Sku,
                cancellationToken
                );
        if (skuAlreadyExists)
        {
            throw new BusinessRuleException(CreateProductErrors.SkuAlreadyExistsCode, CreateProductErrors.SkuAlreadyExistsMessage);
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
            throw new BusinessRuleException(CreateProductErrors.UnitOfMeasureNotFoundCode, CreateProductErrors.UnitOfMeasureNotFoundMessage);
        }

        if (unitOfMeasure.Status != UnitOfMeasureStatus.Active)
        {

            throw new BusinessRuleException(CreateProductErrors.UnitOfMeasureInactiveCode, CreateProductErrors.UnitOfMeasureInactiveMessage);
        }
        if (product.ProductCategoryId is Guid productCategoryId)
        {
            ProductCategory? productCategory =
                await dbContext.ProductCategories
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    existingCategory =>
                    existingCategory.Id == productCategoryId &&
                    existingCategory.OrganisationId == product.OrganisationId,
                    cancellationToken
                    );

            if (productCategory is null)
            {
                throw new BusinessRuleException(CreateProductErrors.ProductCategoryNotFoundCode, CreateProductErrors.ProductCategoryNotFoundMessage);
            }

            if (productCategory.Status != ProductCategoryStatus.Active)
            {
                throw new BusinessRuleException(CreateProductErrors.ProductCategoryInactiveCode, CreateProductErrors.ProductCategoryInactiveMessage);
            }
        }

        TaxCategory? taxCategory =
            await dbContext.TaxCategories
            .AsNoTracking()
            .SingleOrDefaultAsync(
                existingTaxCategory =>
                existingTaxCategory.Id == product.TaxCategoryId,
                cancellationToken
                );

        if (taxCategory is null)
        {
            throw new BusinessRuleException(CreateProductErrors.TaxCategoryNotFoundCode, CreateProductErrors.TaxCategoryNotFoundMessage);
        }
        if (taxCategory.Status != TaxCategoryStatus.Active)
        {
            throw new BusinessRuleException(CreateProductErrors.TaxCategoryInactiveCode, CreateProductErrors.TaxCategoryInactiveMessage);
        }

        DateOnly creationDate = DateOnly.FromDateTime(createdAt.UtcDateTime);

        bool taxCategoryIsNotEffective =
            taxCategory.EffectiveFrom > creationDate ||
            (taxCategory.EffectiveTo.HasValue &&
            taxCategory.EffectiveTo.Value < creationDate);

        if (taxCategoryIsNotEffective)
        {
            throw new BusinessRuleException(CreateProductErrors.TaxCategoryNotEffectiveCode, CreateProductErrors.TaxCategoryNotEffectiveMessage);
        }


        dbContext.Products.Add(product);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

        }
        catch (DbUpdateException exception)
            when (IsDuplicateSkuException(exception))
        {

            throw new BusinessRuleException(CreateProductErrors.SkuAlreadyExistsCode, CreateProductErrors.SkuAlreadyExistsMessage);
        }
        return new CreateProductResult(
            ProductId: product.Id,
            Sku: product.Sku
            );
    }

    private static bool IsDuplicateSkuException(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException &&
            sqlException.Number is 2601 or 2627 &&
            sqlException.Message.Contains(
                SkuUniqueIndexName,
                StringComparison.OrdinalIgnoreCase
                );
    }
}
