using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

namespace TradeFlow.Modules.Purchasing.Domain.GoodsReceipts;

public sealed class GoodsReceiptLine
{
    public Guid Id { get; private set; }
    public Guid GoodsReceiptId { get; private set; }
    public Guid PurchaseOrderLineId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductSku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public decimal QuantityReceived { get; private set; }

    internal static GoodsReceiptLine Create(
        Guid goodsReceiptId,
        PurchaseOrderLine orderLine) =>
        new()
        {
            Id = Guid.NewGuid(),
            GoodsReceiptId = goodsReceiptId,
            PurchaseOrderLineId = orderLine.Id,
            ProductId = orderLine.ProductId,
            ProductSku = orderLine.ProductSku,
            ProductName = orderLine.ProductName,
            QuantityReceived = orderLine.Quantity,
        };

    private GoodsReceiptLine()
    {
    }
}
