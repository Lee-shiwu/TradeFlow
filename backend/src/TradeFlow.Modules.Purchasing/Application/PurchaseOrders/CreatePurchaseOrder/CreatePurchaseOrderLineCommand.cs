namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.CreatePurchaseOrder;

public sealed record CreatePurchaseOrderLineCommand(
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice);
