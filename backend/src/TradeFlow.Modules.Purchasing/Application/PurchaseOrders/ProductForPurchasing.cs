namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders;

public sealed record ProductForPurchasing(
    Guid ProductId,
    string Sku,
    string Name,
    bool IsActive);
