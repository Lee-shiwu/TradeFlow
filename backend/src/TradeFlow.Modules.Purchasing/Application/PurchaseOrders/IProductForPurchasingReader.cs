namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders;

public interface IProductForPurchasingReader
{
    Task<IReadOnlyDictionary<Guid, ProductForPurchasing>> GetByIdsAsync(
        Guid organisationId,
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken);
}
