using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.GetPurchaseOrderById;

public sealed record GetPurchaseOrderByIdResult(
    Guid PurchaseOrderId,
    Guid SupplierId,
    string SupplierCode,
    string SupplierName,
    string Reference,
    PurchaseOrderStatus Status,
    IReadOnlyList<GetPurchaseOrderLineResult> Lines,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    Guid CreatedBy,
    DateTimeOffset? ConfirmedAt,
    Guid? ConfirmedBy,
    byte[] RowVersion);
