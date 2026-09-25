using TradeFlow.Modules.Purchasing.Domain.GoodsReceipts;

namespace TradeFlow.Modules.Purchasing.Domain.Inventory;

public sealed class StockTransaction
{
    public Guid Id { get; private set; }
    public Guid OrganisationId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductSku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public StockTransactionType Type { get; private set; }
    public decimal Quantity { get; private set; }
    public Guid SourceDocumentId { get; private set; }
    public Guid SourceLineId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    public static StockTransaction FromPurchaseReceipt(
        GoodsReceipt receipt,
        GoodsReceiptLine line) =>
        new()
        {
            Id = Guid.NewGuid(),
            OrganisationId = receipt.OrganisationId,
            ProductId = line.ProductId,
            ProductSku = line.ProductSku,
            ProductName = line.ProductName,
            Type = StockTransactionType.PurchaseReceipt,
            Quantity = line.QuantityReceived,
            SourceDocumentId = receipt.Id,
            SourceLineId = line.Id,
            OccurredAt = receipt.ReceivedAt,
            CreatedBy = receipt.ReceivedBy,
        };

    private StockTransaction()
    {
    }
}
