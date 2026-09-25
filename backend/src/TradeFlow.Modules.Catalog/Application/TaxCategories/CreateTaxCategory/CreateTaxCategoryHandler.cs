using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.Modules.Catalog.Application.TaxCategories.CreateTaxCategory;

public sealed class CreateTaxCategoryHandler(CatalogDbContext dbContext)
{
    private const string CodeUniqueIndexName =
        "UX_TaxCategories_Code";

    public async Task<CreateTaxCategoryResult> HandleAsync(
        CreateTaxCategoryCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        TaxCategory category =
            TaxCategory.Create(
                command.Code,
                command.Name,
                command.Description,
                command.Rate,
                command.Treatment,
                command.EffectiveFrom,
                command.EffectiveTo);

        bool codeAlreadyExists =
            await dbContext.TaxCategories.AnyAsync(
                existingCategory => existingCategory.Code == category.Code,
                cancellationToken);

        if (codeAlreadyExists)
        {
            throw CreateDuplicateCodeException();
        }

        dbContext.TaxCategories.Add(category);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsDuplicateCodeException(exception))
        {
            throw CreateDuplicateCodeException();
        }

        return new CreateTaxCategoryResult(
            category.Id,
            category.Code,
            category.Name,
            category.Description,
            category.Rate,
            category.Treatment,
            category.Status,
            category.EffectiveFrom,
            category.EffectiveTo,
            category.RowVersion);
    }

    private static BusinessRuleException CreateDuplicateCodeException()
    {
        return new BusinessRuleException(
            CreateTaxCategoryErrors.CodeAlreadyExistsCode,
            CreateTaxCategoryErrors.CodeAlreadyExistsMessage);
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
