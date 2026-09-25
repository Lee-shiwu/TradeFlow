namespace TradeFlow.Modules.Purchasing.Application.Inventory.ListStock;

public sealed record ListStockResult(
    IReadOnlyList<StockItem> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
