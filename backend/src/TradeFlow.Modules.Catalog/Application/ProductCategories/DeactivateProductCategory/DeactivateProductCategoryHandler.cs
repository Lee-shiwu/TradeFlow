using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.Modules.Catalog.Application.ProductCategories.DeactivateProductCategory;

public sealed class DeactivateProductCategoryHandler(
    CatalogDbContext dbContext,
    IClock clock)
{
    public async Task<DeactivateProductCategoryResult> HandleAsync(
        DeactivateProductCategoryCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);

        ProductCategory? category =
            await dbContext.ProductCategories.SingleOrDefaultAsync(
                existingCategory =>
                    existingCategory.OrganisationId == command.OrganisationId
                    && existingCategory.Id == command.ProductCategoryId,
                cancellationToken);

        if (category is null)
        {
            throw new NotFoundException(
                DeactivateProductCategoryErrors.ProductCategoryNotFoundCode,
                DeactivateProductCategoryErrors.ProductCategoryNotFoundMessage);
        }

        if (!category.RowVersion.SequenceEqual(command.RowVersion))
        {
            throw CreateConcurrencyException();
        }

        if (category.Status == ProductCategoryStatus.Inactive)
        {
            return CreateResult(category);
        }

        dbContext.Entry(category)
            .Property(existingCategory => existingCategory.RowVersion)
            .OriginalValue = command.RowVersion;

        category.Deactivate(clock.UtcNow, command.ModifiedBy);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw CreateConcurrencyException();
        }

        return CreateResult(category);
    }

    private static DeactivateProductCategoryResult CreateResult(
        ProductCategory category)
    {
        return new DeactivateProductCategoryResult(
            category.Id,
            category.Status,
            category.LastModifiedAt,
            category.LastModifiedBy,
            category.RowVersion);
    }

    private static void ValidateCommand(
        DeactivateProductCategoryCommand command)
    {
        if (command.OrganisationId == Guid.Empty)
        {
            throw new RequestValidationException(
                DeactivateProductCategoryErrors.OrganisationRequiredCode,
                DeactivateProductCategoryErrors.OrganisationRequiredMessage);
        }

        if (command.ProductCategoryId == Guid.Empty)
        {
            throw new RequestValidationException(
                DeactivateProductCategoryErrors.ProductCategoryIdRequiredCode,
                DeactivateProductCategoryErrors.ProductCategoryIdRequiredMessage);
        }

        if (command.ModifiedBy == Guid.Empty)
        {
            throw new RequestValidationException(
                DeactivateProductCategoryErrors.ModifiedByRequiredCode,
                DeactivateProductCategoryErrors.ModifiedByRequiredMessage);
        }

        if (command.RowVersion is null || command.RowVersion.Length == 0)
        {
            throw new RequestValidationException(
                DeactivateProductCategoryErrors.RowVersionRequiredCode,
                DeactivateProductCategoryErrors.RowVersionRequiredMessage);
        }
    }

    private static BusinessRuleException CreateConcurrencyException()
    {
        return new BusinessRuleException(
            DeactivateProductCategoryErrors.ConcurrencyConflictCode,
            DeactivateProductCategoryErrors.ConcurrencyConflictMessage);
    }
}
