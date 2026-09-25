using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Purchasing.Domain.Suppliers;
using TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

namespace TradeFlow.Modules.Purchasing.Application.Suppliers.CreateSupplier;

public sealed class CreateSupplierHandler(
    PurchasingDbContext dbContext,
    IClock clock)
{
    private const string CodeUniqueIndexName =
        "UX_Suppliers_OrganisationId_Code";

    public async Task<CreateSupplierResult> HandleAsync(
        CreateSupplierCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        Supplier supplier =
            Supplier.Create(
                command.OrganisationId,
                command.Code,
                command.Name,
                clock.UtcNow,
                command.CreatedBy);

        bool codeAlreadyExists =
            await dbContext.Suppliers.AnyAsync(
                existingSupplier =>
                    existingSupplier.OrganisationId == supplier.OrganisationId
                    && existingSupplier.Code == supplier.Code,
                cancellationToken);

        if (codeAlreadyExists)
        {
            throw CreateDuplicateCodeException();
        }

        dbContext.Suppliers.Add(supplier);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsDuplicateCodeException(exception))
        {
            throw CreateDuplicateCodeException();
        }

        return new CreateSupplierResult(
            supplier.Id,
            supplier.Code,
            supplier.Name,
            supplier.Status,
            supplier.CreatedAt,
            supplier.CreatedBy,
            supplier.RowVersion);
    }

    private static BusinessRuleException CreateDuplicateCodeException()
    {
        return new BusinessRuleException(
            CreateSupplierErrors.CodeAlreadyExistsCode,
            CreateSupplierErrors.CodeAlreadyExistsMessage);
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
