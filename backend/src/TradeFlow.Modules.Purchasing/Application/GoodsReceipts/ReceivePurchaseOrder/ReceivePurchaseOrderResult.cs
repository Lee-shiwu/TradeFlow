namespace TradeFlow.Modules.Purchasing.Application.GoodsReceipts.ReceivePurchaseOrder;

public sealed record ReceivePurchaseOrderResult(
    Guid GoodsReceiptId,
    string ReceiptNumber,
    Guid PurchaseOrderId,
    IReadOnlyList<ReceivePurchaseOrderLineResult> Lines,
    DateTimeOffset ReceivedAt,
    Guid ReceivedBy,
    byte[] PurchaseOrderRowVersion);
