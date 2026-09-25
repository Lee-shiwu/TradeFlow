using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;
using TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.ListPurchaseOrders;

public sealed class ListPurchaseOrdersHandler(PurchasingDbContext dbContext)
{
    public async Task<ListPurchaseOrdersResult> HandleAsync(
        ListPurchaseOrdersQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        Validate(query);

        string? search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        IQueryable<PurchaseOrder> orders =
            dbContext.PurchaseOrders.AsNoTracking()
                .Where(order => order.OrganisationId == query.OrganisationId);

        if (search is not null)
        {
            orders = orders.Where(order => order.Reference.Contains(search));
        }

        if (query.Status.HasValue)
        {
            orders = orders.Where(order => order.Status == query.Status.Value);
        }

        int totalCount = await orders.CountAsync(cancellationToken);
        int skip = checked((query.PageNumber - 1) * query.PageSize);

        List<ListPurchaseOrderItem> items =
            await (
                from order in orders
                join supplier in dbContext.Suppliers.AsNoTracking()
                    on order.SupplierId equals supplier.Id
                orderby order.CreatedAt descending, order.Id
                select new ListPurchaseOrderItem(
                    order.Id,
                    order.SupplierId,
                    supplier.Code,
                    supplier.Name,
                    order.Reference,
                    order.Status,
                    order.Lines.Count,
                    order.Lines.Sum(line => line.Quantity * line.UnitPrice),
                    order.CreatedAt))
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

        int totalPages = totalCount == 0 ? 0 : ((totalCount - 1) / query.PageSize) + 1;
        return new ListPurchaseOrdersResult(
            items,
            query.PageNumber,
            query.PageSize,
            totalCount,
            totalPages);
    }

    private static void Validate(ListPurchaseOrdersQuery query)
    {
        if (query.OrganisationId == Guid.Empty)
        {
            throw new RequestValidationException(
                "LIST_PURCHASE_ORDERS_ORGANISATION_REQUIRED",
                "Organisation is required.");
        }

        if (query.PageNumber < 1 || query.PageSize is < 1 or > 100)
        {
            throw new RequestValidationException(
                "LIST_PURCHASE_ORDERS_PAGING_INVALID",
                "Page number must be at least 1 and page size must be between 1 and 100.");
        }

        if (query.Search?.Trim().Length > 100)
        {
            throw new RequestValidationException(
                "LIST_PURCHASE_ORDERS_SEARCH_INVALID",
                "Search must not exceed 100 characters.");
        }
    }
}
