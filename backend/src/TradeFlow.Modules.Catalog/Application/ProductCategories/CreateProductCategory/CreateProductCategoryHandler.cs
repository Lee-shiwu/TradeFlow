using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.Modules.Catalog.Application.ProductCategories.CreateProductCategory;

public sealed class CreateProductCategoryHandler(
    CatalogDbContext dbContext,
    IClock clock)
{
    private const string CodeUniqueIndexName =
        "UX_ProductCategories_OrganisationId_Code";

    public async Task<CreateProductCategoryResult> HandleAsync(
        CreateProductCategoryCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        ProductCategory category =
            ProductCategory.Create(
                organisationId: command.OrganisationId,
                code: command.Code,
                name: command.Name,
                description: command.Description,
                createdAt: clock.UtcNow,
                createdBy: command.CreatedBy);

        bool codeAlreadyExists =
            await dbContext.ProductCategories.AnyAsync(
                existingCategory =>
                    existingCategory.OrganisationId == category.OrganisationId
                    && existingCategory.Code == category.Code,
                cancellationToken);

        if (codeAlreadyExists)
        {
            throw CreateDuplicateCodeException();
        }

        dbContext.ProductCategories.Add(category);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsDuplicateCodeException(exception))
        {
            throw CreateDuplicateCodeException();
        }

        return new CreateProductCategoryResult(
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

    private static BusinessRuleException CreateDuplicateCodeException()
    {
        return new BusinessRuleException(
            CreateProductCategoryErrors.CodeAlreadyExistsCode,
            CreateProductCategoryErrors.CodeAlreadyExistsMessage);
    }

    private static bool IsDuplicateCodeException(
        DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && sqlException.Number is 2601 or 2627
            && sqlException.Message.Contains(
                CodeUniqueIndexName,
                StringComparison.OrdinalIgnoreCase);
    }
}
