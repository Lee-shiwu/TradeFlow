namespace TradeFlow.Api.Endpoints.Inventory;

public sealed record ListStockResponse(
    IReadOnlyList<StockItemResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record StockItemResponse(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    decimal QuantityOnHand,
    DateTimeOffset LastMovementAt);
