using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

namespace TradeFlow.UnitTests.Purchasing.PurchaseOrders;

public sealed class PurchaseOrderCreateTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 9, 25, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidLines_CreatesDraftAndCalculatesTotal()
    {
        Guid organisationId = Guid.NewGuid();
        Guid supplierId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();

        PurchaseOrder order = PurchaseOrder.Create(
            organisationId,
            supplierId,
            " po-1001 ",
            [new PurchaseOrderLineDraft(productId, "SKU-1", "Product one", 2.5m, 12m)],
            CreatedAt,
            Guid.NewGuid());

        Assert.Equal("PO-1001", order.Reference);
        Assert.Equal(PurchaseOrderStatus.Draft, order.Status);
        Assert.Equal(30m, order.TotalAmount);
        PurchaseOrderLine line = Assert.Single(order.Lines);
        Assert.Equal(productId, line.ProductId);
        Assert.Equal(30m, line.LineTotal);
    }

    [Fact]
    public void Create_WithNoLines_ThrowsBusinessRule()
    {
        BusinessRuleException exception = Assert.Throws<BusinessRuleException>(() =>
            PurchaseOrder.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "PO-1",
                [],
                CreatedAt,
                Guid.NewGuid()));

        Assert.Equal("PURCHASE_ORDER_LINES_REQUIRED", exception.Code);
    }

    [Fact]
    public void Create_WithDuplicateProduct_ThrowsBusinessRule()
    {
        Guid productId = Guid.NewGuid();
        BusinessRuleException exception = Assert.Throws<BusinessRuleException>(() =>
            PurchaseOrder.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "PO-1",
                [
                    new PurchaseOrderLineDraft(productId, "SKU", "Product", 1m, 1m),
                    new PurchaseOrderLineDraft(productId, "SKU", "Product", 2m, 1m),
                ],
                CreatedAt,
                Guid.NewGuid()));

        Assert.Equal("PURCHASE_ORDER_DUPLICATE_PRODUCT", exception.Code);
    }

    [Theory]
    [InlineData(0, 1, "PURCHASE_ORDER_QUANTITY_INVALID")]
    [InlineData(1, -1, "PURCHASE_ORDER_UNIT_PRICE_INVALID")]
    public void Create_WithInvalidAmounts_ThrowsBusinessRule(
        decimal quantity,
        decimal unitPrice,
        string expectedCode)
    {
        BusinessRuleException exception = Assert.Throws<BusinessRuleException>(() =>
            PurchaseOrder.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "PO-1",
                [new PurchaseOrderLineDraft(Guid.NewGuid(), "SKU", "Product", quantity, unitPrice)],
                CreatedAt,
                Guid.NewGuid()));

        Assert.Equal(expectedCode, exception.Code);
    }

    [Fact]
    public void Confirm_WithDraftOrder_RecordsAuditAndChangesStatus()
    {
        PurchaseOrder order = CreateValidOrder();
        Guid confirmedBy = Guid.NewGuid();
        DateTimeOffset confirmedAt = CreatedAt.AddHours(1);

        order.Confirm(confirmedAt, confirmedBy);

        Assert.Equal(PurchaseOrderStatus.Confirmed, order.Status);
        Assert.Equal(confirmedAt, order.ConfirmedAt);
        Assert.Equal(confirmedBy, order.ConfirmedBy);
    }

    [Fact]
    public void Confirm_WithConfirmedOrder_ThrowsBusinessRule()
    {
        PurchaseOrder order = CreateValidOrder();
        order.Confirm(CreatedAt.AddHours(1), Guid.NewGuid());

        BusinessRuleException exception = Assert.Throws<BusinessRuleException>(() =>
            order.Confirm(CreatedAt.AddHours(2), Guid.NewGuid()));

        Assert.Equal("PURCHASE_ORDER_ALREADY_CONFIRMED", exception.Code);
    }

    [Fact]
    public void Receive_WithConfirmedOrder_RecordsAuditAndChangesStatus()
    {
        PurchaseOrder order = CreateValidOrder();
        order.Confirm(CreatedAt.AddHours(1), Guid.NewGuid());
        Guid receivedBy = Guid.NewGuid();
        DateTimeOffset receivedAt = CreatedAt.AddHours(2);

        order.Receive(receivedAt, receivedBy);

        Assert.Equal(PurchaseOrderStatus.Received, order.Status);
        Assert.Equal(receivedAt, order.ReceivedAt);
        Assert.Equal(receivedBy, order.ReceivedBy);
    }

    [Fact]
    public void Receive_WithDraftOrder_ThrowsBusinessRule()
    {
        PurchaseOrder order = CreateValidOrder();

        BusinessRuleException exception = Assert.Throws<BusinessRuleException>(() =>
            order.Receive(CreatedAt.AddHours(1), Guid.NewGuid()));

        Assert.Equal("PURCHASE_ORDER_NOT_CONFIRMED", exception.Code);
    }

    private static PurchaseOrder CreateValidOrder() =>
        PurchaseOrder.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "PO-1",
            [new PurchaseOrderLineDraft(Guid.NewGuid(), "SKU", "Product", 1m, 1m)],
            CreatedAt,
            Guid.NewGuid());
}
