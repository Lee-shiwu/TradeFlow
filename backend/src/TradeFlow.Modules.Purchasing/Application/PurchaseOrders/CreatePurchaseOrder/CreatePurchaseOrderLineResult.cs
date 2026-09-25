namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.CreatePurchaseOrder;

public sealed record CreatePurchaseOrderLineResult(
    Guid LineId,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);
