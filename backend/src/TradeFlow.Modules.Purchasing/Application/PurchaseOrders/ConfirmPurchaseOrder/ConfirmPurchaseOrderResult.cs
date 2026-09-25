using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.ConfirmPurchaseOrder;

public sealed record ConfirmPurchaseOrderResult(
    Guid PurchaseOrderId,
    PurchaseOrderStatus Status,
    DateTimeOffset ConfirmedAt,
    Guid ConfirmedBy,
    byte[] RowVersion);
