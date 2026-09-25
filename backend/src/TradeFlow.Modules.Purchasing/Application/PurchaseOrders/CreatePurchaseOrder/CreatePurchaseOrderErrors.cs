namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.CreatePurchaseOrder;

internal static class CreatePurchaseOrderErrors
{
    internal const string SupplierNotFoundCode = "PURCHASE_ORDER_SUPPLIER_NOT_FOUND";
    internal const string SupplierNotFoundMessage = "The supplier was not found for this organisation.";
    internal const string SupplierInactiveCode = "PURCHASE_ORDER_SUPPLIER_INACTIVE";
    internal const string SupplierInactiveMessage = "The supplier must be active.";
    internal const string ProductNotFoundCode = "PURCHASE_ORDER_PRODUCT_NOT_FOUND";
    internal const string ProductNotFoundMessage = "One or more products were not found for this organisation.";
    internal const string ProductInactiveCode = "PURCHASE_ORDER_PRODUCT_INACTIVE";
    internal const string ProductInactiveMessage = "All purchase order products must be active.";
    internal const string ReferenceAlreadyExistsCode = "PURCHASE_ORDER_REFERENCE_ALREADY_EXISTS";
    internal const string ReferenceAlreadyExistsMessage = "A purchase order with this reference already exists.";
}
