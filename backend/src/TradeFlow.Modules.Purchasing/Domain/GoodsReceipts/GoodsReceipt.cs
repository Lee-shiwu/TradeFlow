using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

namespace TradeFlow.Modules.Purchasing.Domain.GoodsReceipts;

public sealed class GoodsReceipt
{
    private readonly List<GoodsReceiptLine> _lines = [];

    public Guid Id { get; private set; }
    public Guid OrganisationId { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public string ReceiptNumber { get; private set; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; private set; }
    public Guid ReceivedBy { get; private set; }
    public IReadOnlyCollection<GoodsReceiptLine> Lines => _lines;

    public static GoodsReceipt Create(
        PurchaseOrder order,
        DateTimeOffset receivedAt,
        Guid receivedBy)
    {
        GoodsReceipt receipt = new()
        {
            Id = Guid.NewGuid(),
            OrganisationId = order.OrganisationId,
            PurchaseOrderId = order.Id,
            ReceiptNumber = $"GR-{Guid.NewGuid():N}".ToUpperInvariant(),
            ReceivedAt = receivedAt.ToUniversalTime(),
            ReceivedBy = receivedBy,
        };

        receipt._lines.AddRange(
            order.Lines.Select(line => GoodsReceiptLine.Create(receipt.Id, line)));
        return receipt;
    }

    private GoodsReceipt()
    {
    }
}
