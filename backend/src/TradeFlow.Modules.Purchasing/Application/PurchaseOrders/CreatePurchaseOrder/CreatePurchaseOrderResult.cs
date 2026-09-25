using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.CreatePurchaseOrder;

public sealed record CreatePurchaseOrderResult(
    Guid PurchaseOrderId,
    Guid SupplierId,
    string SupplierCode,
    string SupplierName,
    string Reference,
    PurchaseOrderStatus Status,
    IReadOnlyList<CreatePurchaseOrderLineResult> Lines,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    Guid CreatedBy,
    byte[] RowVersion);
