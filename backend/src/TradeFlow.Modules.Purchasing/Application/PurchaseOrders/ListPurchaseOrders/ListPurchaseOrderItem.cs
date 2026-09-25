using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.ListPurchaseOrders;

public sealed record ListPurchaseOrderItem(
    Guid PurchaseOrderId,
    Guid SupplierId,
    string SupplierCode,
    string SupplierName,
    string Reference,
    PurchaseOrderStatus Status,
    int LineCount,
    decimal TotalAmount,
    DateTimeOffset CreatedAt);
