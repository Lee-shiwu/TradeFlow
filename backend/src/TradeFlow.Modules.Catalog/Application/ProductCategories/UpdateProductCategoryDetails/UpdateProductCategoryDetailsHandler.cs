using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.Modules.Catalog.Application.ProductCategories.UpdateProductCategoryDetails;

public sealed class UpdateProductCategoryDetailsHandler(
    CatalogDbContext dbContext,
    IClock clock)
{
    public async Task<UpdateProductCategoryDetailsResult> HandleAsync(
        UpdateProductCategoryDetailsCommand command,
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
                UpdateProductCategoryDetailsErrors.ProductCategoryNotFoundCode,
                UpdateProductCategoryDetailsErrors.ProductCategoryNotFoundMessage);
        }

        if (!category.RowVersion.SequenceEqual(command.RowVersion))
        {
            throw CreateConcurrencyException();
        }

        dbContext.Entry(category)
            .Property(existingCategory => existingCategory.RowVersion)
            .OriginalValue = command.RowVersion;

        category.UpdateDetails(
            name: command.Name,
            description: command.Description,
            modifiedAt: clock.UtcNow,
            modifiedBy: command.ModifiedBy);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw CreateConcurrencyException();
        }

        return new UpdateProductCategoryDetailsResult(
            ProductCategoryId: category.Id,
            Code: category.Code,
            Name: category.Name,
            Description: category.Description,
            Status: category.Status,
            CreatedAt: category.CreatedAt,
            CreatedBy: category.CreatedBy,
            LastModifiedAt: category.LastModifiedAt,
            LastModifiedBy: category.LastModifiedBy,
            RowVersion: category.RowVersion);
    }

    private static void ValidateCommand(
        UpdateProductCategoryDetailsCommand command)
    {
        if (command.OrganisationId == Guid.Empty)
        {
            throw new RequestValidationException(
                UpdateProductCategoryDetailsErrors.OrganisationRequiredCode,
                UpdateProductCategoryDetailsErrors.OrganisationRequiredMessage);
        }

        if (command.ProductCategoryId == Guid.Empty)
        {
            throw new RequestValidationException(
                UpdateProductCategoryDetailsErrors.ProductCategoryIdRequiredCode,
                UpdateProductCategoryDetailsErrors.ProductCategoryIdRequiredMessage);
        }

        if (command.ModifiedBy == Guid.Empty)
        {
            throw new RequestValidationException(
                UpdateProductCategoryDetailsErrors.ModifiedByRequiredCode,
                UpdateProductCategoryDetailsErrors.ModifiedByRequiredMessage);
        }

        if (command.RowVersion is null || command.RowVersion.Length == 0)
        {
            throw new RequestValidationException(
                UpdateProductCategoryDetailsErrors.RowVersionRequiredCode,
                UpdateProductCategoryDetailsErrors.RowVersionRequiredMessage);
        }
    }

    private static BusinessRuleException CreateConcurrencyException()
    {
        return new BusinessRuleException(
            UpdateProductCategoryDetailsErrors.ConcurrencyConflictCode,
            UpdateProductCategoryDetailsErrors.ConcurrencyConflictMessage);
    }
}
