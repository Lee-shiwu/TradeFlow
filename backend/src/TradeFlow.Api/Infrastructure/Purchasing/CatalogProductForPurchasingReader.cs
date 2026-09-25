using Microsoft.EntityFrameworkCore;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;
using TradeFlow.Modules.Purchasing.Application.PurchaseOrders;

namespace TradeFlow.Api.Infrastructure.Purchasing;

public sealed class CatalogProductForPurchasingReader(CatalogDbContext dbContext)
    : IProductForPurchasingReader
{
    public async Task<IReadOnlyDictionary<Guid, ProductForPurchasing>> GetByIdsAsync(
        Guid organisationId,
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        List<ProductForPurchasing> products =
            await dbContext.Products.AsNoTracking()
                .Where(product =>
                    product.OrganisationId == organisationId
                    && productIds.Contains(product.Id))
                .Select(product =>
                    new ProductForPurchasing(
                        product.Id,
                        product.Sku,
                        product.Name,
                        product.Status == ProductStatus.Active))
                .ToListAsync(cancellationToken);

        return products.ToDictionary(product => product.ProductId);
    }
}
