namespace TradeFlow.Api.Endpoints.Purchasing.PurchaseOrders;

public sealed record CreatePurchaseOrderRequest(
    Guid SupplierId,
    string Reference,
    IReadOnlyList<CreatePurchaseOrderLineRequest> Lines);

public sealed record CreatePurchaseOrderLineRequest(
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice);
