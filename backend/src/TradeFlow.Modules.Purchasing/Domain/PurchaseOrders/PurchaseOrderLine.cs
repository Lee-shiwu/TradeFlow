using TradeFlow.BuildingBlocks.Exceptions;

namespace TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

public sealed class PurchaseOrderLine
{
    public Guid Id { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductSku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => Quantity * UnitPrice;

    internal static PurchaseOrderLine Create(
        Guid purchaseOrderId,
        PurchaseOrderLineDraft draft)
    {
        if (draft.ProductId == Guid.Empty)
        {
            throw new BusinessRuleException(
                PurchaseOrderErrors.ProductRequiredCode,
                PurchaseOrderErrors.ProductRequiredMessage);
        }

        if (draft.Quantity <= 0 || decimal.Round(draft.Quantity, 4) != draft.Quantity)
        {
            throw new BusinessRuleException(
                PurchaseOrderErrors.QuantityInvalidCode,
                PurchaseOrderErrors.QuantityInvalidMessage);
        }

        if (draft.UnitPrice < 0 || decimal.Round(draft.UnitPrice, 4) != draft.UnitPrice)
        {
            throw new BusinessRuleException(
                PurchaseOrderErrors.UnitPriceInvalidCode,
                PurchaseOrderErrors.UnitPriceInvalidMessage);
        }

        return new PurchaseOrderLine
        {
            Id = Guid.NewGuid(),
            PurchaseOrderId = purchaseOrderId,
            ProductId = draft.ProductId,
            ProductSku = draft.ProductSku,
            ProductName = draft.ProductName,
            Quantity = draft.Quantity,
            UnitPrice = draft.UnitPrice,
        };
    }

    private PurchaseOrderLine()
    {
    }
}
