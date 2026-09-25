namespace TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

public sealed record PurchaseOrderLineDraft(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice);
