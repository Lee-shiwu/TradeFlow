using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Purchasing.Domain.Suppliers;
using TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

namespace TradeFlow.Modules.Purchasing.Application.Suppliers.ListSuppliers;

public sealed class ListSuppliersHandler(PurchasingDbContext dbContext)
{
    public async Task<ListSuppliersResult> HandleAsync(
        ListSuppliersQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        ValidateQuery(query);
        string? normalizedSearch = NormalizeSearch(query.Search);

        IQueryable<Supplier> suppliersQuery =
            dbContext.Suppliers
                .AsNoTracking()
                .Where(supplier =>
                    supplier.OrganisationId == query.OrganisationId);

        if (normalizedSearch is not null)
        {
            suppliersQuery = suppliersQuery.Where(supplier =>
                supplier.Code.Contains(normalizedSearch)
                || supplier.Name.Contains(normalizedSearch));
        }

        if (query.Status.HasValue)
        {
            suppliersQuery = suppliersQuery.Where(supplier =>
                supplier.Status == query.Status.Value);
        }

        int totalCount =
            await suppliersQuery.CountAsync(cancellationToken);
        int skipCount = checked((query.PageNumber - 1) * query.PageSize);

        List<ListSupplierItem> items =
            await suppliersQuery
                .OrderBy(supplier => supplier.Code)
                .ThenBy(supplier => supplier.Id)
                .Skip(skipCount)
                .Take(query.PageSize)
                .Select(supplier =>
                    new ListSupplierItem(
                        supplier.Id,
                        supplier.Code,
                        supplier.Name,
                        supplier.Status,
                        supplier.CreatedAt))
                .ToListAsync(cancellationToken);

        int totalPages =
            totalCount == 0
                ? 0
                : ((totalCount - 1) / query.PageSize) + 1;

        return new ListSuppliersResult(
            items,
            query.PageNumber,
            query.PageSize,
            totalCount,
            totalPages);
    }

    private static void ValidateQuery(ListSuppliersQuery query)
    {
        if (query.OrganisationId == Guid.Empty)
        {
            throw new RequestValidationException(
                ListSuppliersErrors.OrganisationRequiredCode,
                ListSuppliersErrors.OrganisationRequiredMessage);
        }

        if (query.PageNumber < 1)
        {
            throw new RequestValidationException(
                ListSuppliersErrors.PageNumberInvalidCode,
                ListSuppliersErrors.PageNumberInvalidMessage);
        }

        if (query.PageSize < 1
            || query.PageSize > ListSuppliersErrors.MaxPageSize)
        {
            throw new RequestValidationException(
                ListSuppliersErrors.PageSizeInvalidCode,
                ListSuppliersErrors.PageSizeInvalidMessage);
        }
    }

    private static string? NormalizeSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        string normalizedSearch = search.Trim();

        if (normalizedSearch.Length > ListSuppliersErrors.MaxSearchLength)
        {
            throw new RequestValidationException(
                ListSuppliersErrors.SearchInvalidCode,
                ListSuppliersErrors.SearchInvalidMessage);
        }

        return normalizedSearch;
    }
}
