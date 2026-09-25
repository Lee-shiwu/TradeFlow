using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

namespace TradeFlow.Api.Endpoints.Purchasing.PurchaseOrders;

public sealed class ListPurchaseOrdersRequest
{
    public string? Search { get; init; }
    public PurchaseOrderStatus? Status { get; init; }
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
}
