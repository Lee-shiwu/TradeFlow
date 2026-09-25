namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.GetPurchaseOrderById;

public sealed record GetPurchaseOrderLineResult(
    Guid LineId,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);
