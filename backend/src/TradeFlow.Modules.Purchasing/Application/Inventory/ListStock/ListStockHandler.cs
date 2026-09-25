using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

namespace TradeFlow.Modules.Purchasing.Application.Inventory.ListStock;

public sealed class ListStockHandler(PurchasingDbContext dbContext)
{
    public async Task<ListStockResult> HandleAsync(
        ListStockQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.OrganisationId == Guid.Empty || query.PageNumber < 1 || query.PageSize is < 1 or > 100)
        {
            throw new RequestValidationException(
                "LIST_STOCK_REQUEST_INVALID",
                "Organisation and valid paging values are required.");
        }

        string? search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        var transactions = dbContext.StockTransactions.AsNoTracking()
            .Where(item => item.OrganisationId == query.OrganisationId);
        if (search is not null)
        {
            transactions = transactions.Where(item =>
                item.ProductSku.Contains(search) || item.ProductName.Contains(search));
        }

        var stockQuery = transactions
            .GroupBy(item => new { item.ProductId, item.ProductSku, item.ProductName })
            .Select(group => new
            {
                group.Key.ProductId,
                group.Key.ProductSku,
                group.Key.ProductName,
                QuantityOnHand = group.Sum(item => item.Quantity),
                LastMovementAt = group.Max(item => item.OccurredAt),
            });
        int totalCount = await stockQuery.CountAsync(cancellationToken);
        var rows = await stockQuery
            .OrderBy(item => item.ProductSku)
            .ThenBy(item => item.ProductId)
            .Skip(checked((query.PageNumber - 1) * query.PageSize))
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
        List<StockItem> items = rows.Select(item => new StockItem(
            item.ProductId,
            item.ProductSku,
            item.ProductName,
            item.QuantityOnHand,
            item.LastMovementAt)).ToList();
        int totalPages = totalCount == 0 ? 0 : ((totalCount - 1) / query.PageSize) + 1;
        return new ListStockResult(items, query.PageNumber, query.PageSize, totalCount, totalPages);
    }
}
