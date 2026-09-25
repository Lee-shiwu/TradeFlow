using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.ListPurchaseOrders;

public sealed record ListPurchaseOrdersQuery(
    Guid OrganisationId,
    string? Search,
    PurchaseOrderStatus? Status,
    int PageNumber,
    int PageSize);
