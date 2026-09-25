namespace TradeFlow.Modules.Purchasing.Application.GoodsReceipts.ReceivePurchaseOrder;

public sealed record ReceivePurchaseOrderLineResult(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    decimal QuantityReceived);
