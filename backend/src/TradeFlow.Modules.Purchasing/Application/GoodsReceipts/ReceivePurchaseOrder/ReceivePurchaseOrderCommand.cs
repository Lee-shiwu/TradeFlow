namespace TradeFlow.Modules.Purchasing.Application.GoodsReceipts.ReceivePurchaseOrder;

public sealed record ReceivePurchaseOrderCommand(
    Guid OrganisationId,
    Guid PurchaseOrderId,
    Guid ReceivedBy,
    byte[] RowVersion);
