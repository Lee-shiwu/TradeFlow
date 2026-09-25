namespace TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

internal static class PurchaseOrderErrors
{
    internal const string OrganisationRequiredCode = "PURCHASE_ORDER_ORGANISATION_REQUIRED";
    internal const string OrganisationRequiredMessage = "Organisation is required.";
    internal const string SupplierRequiredCode = "PURCHASE_ORDER_SUPPLIER_REQUIRED";
    internal const string SupplierRequiredMessage = "Supplier is required.";
    internal const string ReferenceRequiredCode = "PURCHASE_ORDER_REFERENCE_REQUIRED";
    internal const string ReferenceRequiredMessage = "Order reference is required.";
    internal const string ReferenceInvalidCode = "PURCHASE_ORDER_REFERENCE_INVALID";
    internal const string ReferenceInvalidMessage = "Order reference must not exceed 50 characters.";
    internal const string LinesRequiredCode = "PURCHASE_ORDER_LINES_REQUIRED";
    internal const string LinesRequiredMessage = "At least one purchase order line is required.";
    internal const string ProductRequiredCode = "PURCHASE_ORDER_PRODUCT_REQUIRED";
    internal const string ProductRequiredMessage = "A product is required for every purchase order line.";
    internal const string DuplicateProductCode = "PURCHASE_ORDER_DUPLICATE_PRODUCT";
    internal const string DuplicateProductMessage = "A product may appear only once on a purchase order.";
    internal const string QuantityInvalidCode = "PURCHASE_ORDER_QUANTITY_INVALID";
    internal const string QuantityInvalidMessage = "Quantity must be greater than zero and have at most four decimal places.";
    internal const string UnitPriceInvalidCode = "PURCHASE_ORDER_UNIT_PRICE_INVALID";
    internal const string UnitPriceInvalidMessage = "Unit price must be zero or greater and have at most four decimal places.";
    internal const string CreatedByRequiredCode = "PURCHASE_ORDER_CREATED_BY_REQUIRED";
    internal const string CreatedByRequiredMessage = "Created by is required.";
    internal const string ConfirmedByRequiredCode = "PURCHASE_ORDER_CONFIRMED_BY_REQUIRED";
    internal const string ConfirmedByRequiredMessage = "Confirmed by is required.";
    internal const string AlreadyConfirmedCode = "PURCHASE_ORDER_ALREADY_CONFIRMED";
    internal const string AlreadyConfirmedMessage = "The purchase order is already confirmed.";
    internal const string NotConfirmedCode = "PURCHASE_ORDER_NOT_CONFIRMED";
    internal const string NotConfirmedMessage = "Only a confirmed purchase order can be received.";
    internal const string ReceivedByRequiredCode = "PURCHASE_ORDER_RECEIVED_BY_REQUIRED";
    internal const string ReceivedByRequiredMessage = "Received by is required.";
}
