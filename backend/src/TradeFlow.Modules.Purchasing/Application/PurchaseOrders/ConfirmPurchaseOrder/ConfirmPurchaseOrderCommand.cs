namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.ConfirmPurchaseOrder;

public sealed record ConfirmPurchaseOrderCommand(
    Guid OrganisationId,
    Guid PurchaseOrderId,
    Guid ConfirmedBy,
    byte[] RowVersion);
