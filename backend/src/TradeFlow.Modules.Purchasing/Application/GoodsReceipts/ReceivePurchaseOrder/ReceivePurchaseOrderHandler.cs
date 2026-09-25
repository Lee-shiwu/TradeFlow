using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Purchasing.Domain.GoodsReceipts;
using TradeFlow.Modules.Purchasing.Domain.Inventory;
using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;
using TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

namespace TradeFlow.Modules.Purchasing.Application.GoodsReceipts.ReceivePurchaseOrder;

public sealed class ReceivePurchaseOrderHandler(
    PurchasingDbContext dbContext,
    IClock clock)
{
    public async Task<ReceivePurchaseOrderResult> HandleAsync(
        ReceivePurchaseOrderCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        Validate(command);

        PurchaseOrder? order = await dbContext.PurchaseOrders
            .Include(item => item.Lines)
            .SingleOrDefaultAsync(
                item =>
                    item.Id == command.PurchaseOrderId
                    && item.OrganisationId == command.OrganisationId,
                cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(
                ReceivePurchaseOrderErrors.NotFoundCode,
                ReceivePurchaseOrderErrors.NotFoundMessage);
        }

        dbContext.Entry(order).Property(item => item.RowVersion).OriginalValue = command.RowVersion;
        DateTimeOffset receivedAt = clock.UtcNow;
        GoodsReceipt receipt = GoodsReceipt.Create(order, receivedAt, command.ReceivedBy);
        order.Receive(receivedAt, command.ReceivedBy);
        List<StockTransaction> transactions = receipt.Lines
            .Select(line => StockTransaction.FromPurchaseReceipt(receipt, line))
            .ToList();

        dbContext.GoodsReceipts.Add(receipt);
        dbContext.StockTransactions.AddRange(transactions);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BusinessRuleException(
                ReceivePurchaseOrderErrors.ConcurrencyConflictCode,
                ReceivePurchaseOrderErrors.ConcurrencyConflictMessage);
        }

        return new ReceivePurchaseOrderResult(
            receipt.Id,
            receipt.ReceiptNumber,
            order.Id,
            receipt.Lines.Select(line => new ReceivePurchaseOrderLineResult(
                line.ProductId,
                line.ProductSku,
                line.ProductName,
                line.QuantityReceived)).ToList(),
            receipt.ReceivedAt,
            receipt.ReceivedBy,
            order.RowVersion);
    }

    private static void Validate(ReceivePurchaseOrderCommand command)
    {
        if (command.OrganisationId == Guid.Empty
            || command.PurchaseOrderId == Guid.Empty
            || command.ReceivedBy == Guid.Empty)
        {
            throw new RequestValidationException(
                ReceivePurchaseOrderErrors.IdentifierRequiredCode,
                ReceivePurchaseOrderErrors.IdentifierRequiredMessage);
        }

        if (command.RowVersion is null || command.RowVersion.Length == 0)
        {
            throw new RequestValidationException(
                ReceivePurchaseOrderErrors.RowVersionRequiredCode,
                ReceivePurchaseOrderErrors.RowVersionRequiredMessage);
        }
    }
}
