using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.Modules.Catalog.Application.ProductCategories.ActivateProductCategory;

public sealed class ActivateProductCategoryHandler(
    CatalogDbContext dbContext,
    IClock clock)
{
    public async Task<ActivateProductCategoryResult> HandleAsync(
        ActivateProductCategoryCommand command,
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
                ActivateProductCategoryErrors.ProductCategoryNotFoundCode,
                ActivateProductCategoryErrors.ProductCategoryNotFoundMessage);
        }

        if (!category.RowVersion.SequenceEqual(command.RowVersion))
        {
            throw CreateConcurrencyException();
        }

        if (category.Status == ProductCategoryStatus.Active)
        {
            return CreateResult(category);
        }

        dbContext.Entry(category)
            .Property(existingCategory => existingCategory.RowVersion)
            .OriginalValue = command.RowVersion;

        category.Activate(clock.UtcNow, command.ModifiedBy);

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

    private static ActivateProductCategoryResult CreateResult(
        ProductCategory category)
    {
        return new ActivateProductCategoryResult(
            category.Id,
            category.Status,
            category.LastModifiedAt,
            category.LastModifiedBy,
            category.RowVersion);
    }

    private static void ValidateCommand(ActivateProductCategoryCommand command)
    {
        if (command.OrganisationId == Guid.Empty)
        {
            throw new RequestValidationException(
                ActivateProductCategoryErrors.OrganisationRequiredCode,
                ActivateProductCategoryErrors.OrganisationRequiredMessage);
        }

        if (command.ProductCategoryId == Guid.Empty)
        {
            throw new RequestValidationException(
                ActivateProductCategoryErrors.ProductCategoryIdRequiredCode,
                ActivateProductCategoryErrors.ProductCategoryIdRequiredMessage);
        }

        if (command.ModifiedBy == Guid.Empty)
        {
            throw new RequestValidationException(
                ActivateProductCategoryErrors.ModifiedByRequiredCode,
                ActivateProductCategoryErrors.ModifiedByRequiredMessage);
        }

        if (command.RowVersion is null || command.RowVersion.Length == 0)
        {
            throw new RequestValidationException(
                ActivateProductCategoryErrors.RowVersionRequiredCode,
                ActivateProductCategoryErrors.RowVersionRequiredMessage);
        }
    }

    private static BusinessRuleException CreateConcurrencyException()
    {
        return new BusinessRuleException(
            ActivateProductCategoryErrors.ConcurrencyConflictCode,
            ActivateProductCategoryErrors.ConcurrencyConflictMessage);
    }
}
