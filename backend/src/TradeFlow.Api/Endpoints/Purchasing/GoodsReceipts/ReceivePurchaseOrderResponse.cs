namespace TradeFlow.Api.Endpoints.Purchasing.GoodsReceipts;

public sealed record ReceivePurchaseOrderResponse(
    Guid GoodsReceiptId,
    string ReceiptNumber,
    Guid PurchaseOrderId,
    IReadOnlyList<ReceivePurchaseOrderLineResponse> Lines,
    DateTimeOffset ReceivedAt,
    Guid ReceivedBy,
    string PurchaseOrderRowVersion);

public sealed record ReceivePurchaseOrderLineResponse(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    decimal QuantityReceived);
