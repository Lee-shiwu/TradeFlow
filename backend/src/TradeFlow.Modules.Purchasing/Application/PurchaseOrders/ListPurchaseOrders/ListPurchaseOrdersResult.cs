namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.ListPurchaseOrders;

public sealed record ListPurchaseOrdersResult(
    IReadOnlyList<ListPurchaseOrderItem> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
