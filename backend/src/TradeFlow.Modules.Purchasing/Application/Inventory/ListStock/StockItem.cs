namespace TradeFlow.Modules.Purchasing.Application.Inventory.ListStock;

public sealed record StockItem(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    decimal QuantityOnHand,
    DateTimeOffset LastMovementAt);
