using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;
using TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.ConfirmPurchaseOrder;

public sealed class ConfirmPurchaseOrderHandler(
    PurchasingDbContext dbContext,
    IClock clock)
{
    public async Task<ConfirmPurchaseOrderResult> HandleAsync(
        ConfirmPurchaseOrderCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        Validate(command);

        PurchaseOrder? order = await dbContext.PurchaseOrders.SingleOrDefaultAsync(
            item =>
                item.Id == command.PurchaseOrderId
                && item.OrganisationId == command.OrganisationId,
            cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(
                ConfirmPurchaseOrderErrors.NotFoundCode,
                ConfirmPurchaseOrderErrors.NotFoundMessage);
        }

        dbContext.Entry(order).Property(item => item.RowVersion).OriginalValue = command.RowVersion;
        DateTimeOffset confirmedAt = clock.UtcNow;
        order.Confirm(confirmedAt, command.ConfirmedBy);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BusinessRuleException(
                ConfirmPurchaseOrderErrors.ConcurrencyConflictCode,
                ConfirmPurchaseOrderErrors.ConcurrencyConflictMessage);
        }

        return new ConfirmPurchaseOrderResult(
            order.Id,
            order.Status,
            confirmedAt.ToUniversalTime(),
            command.ConfirmedBy,
            order.RowVersion);
    }

    private static void Validate(ConfirmPurchaseOrderCommand command)
    {
        if (command.OrganisationId == Guid.Empty
            || command.PurchaseOrderId == Guid.Empty
            || command.ConfirmedBy == Guid.Empty)
        {
            throw new RequestValidationException(
                ConfirmPurchaseOrderErrors.IdentifierRequiredCode,
                ConfirmPurchaseOrderErrors.IdentifierRequiredMessage);
        }

        if (command.RowVersion is null || command.RowVersion.Length == 0)
        {
            throw new RequestValidationException(
                ConfirmPurchaseOrderErrors.RowVersionRequiredCode,
                ConfirmPurchaseOrderErrors.RowVersionRequiredMessage);
        }
    }
}
