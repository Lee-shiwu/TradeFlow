namespace TradeFlow.Modules.Purchasing.Application.GoodsReceipts.ReceivePurchaseOrder;

internal static class ReceivePurchaseOrderErrors
{
    internal const string IdentifierRequiredCode = "RECEIVE_PURCHASE_ORDER_IDENTIFIER_REQUIRED";
    internal const string IdentifierRequiredMessage = "Organisation, purchase order and user identifiers are required.";
    internal const string RowVersionRequiredCode = "RECEIVE_PURCHASE_ORDER_ROW_VERSION_REQUIRED";
    internal const string RowVersionRequiredMessage = "RowVersion is required.";
    internal const string NotFoundCode = "PURCHASE_ORDER_NOT_FOUND";
    internal const string NotFoundMessage = "The purchase order was not found.";
    internal const string ConcurrencyConflictCode = "PURCHASE_ORDER_CONCURRENCY_CONFLICT";
    internal const string ConcurrencyConflictMessage = "The purchase order was changed by another request. Reload it and try again.";
}
