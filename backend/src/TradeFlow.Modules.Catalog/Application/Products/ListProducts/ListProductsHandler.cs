using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;


namespace TradeFlow.Modules.Catalog.Application.Products.ListProducts;

public sealed class ListProductsHandler(CatalogDbContext dbContext)
{
    public async Task<ListProductsResult> HandleAsync(ListProductsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        ValidateQuery(query);
        string? normalizedSearch = NormalizeSearch(query.Search);
        IQueryable<Product> productsQuery =
            dbContext.Products
            .AsNoTracking()
            .Where(product => product.OrganisationId == query.OrganisationId);

        if (normalizedSearch is not null)
        {
            productsQuery =
                productsQuery.Where(product =>
                product.Sku.Contains(normalizedSearch) ||
                product.Name.Contains(normalizedSearch)
                    );
        }

        if (query.Status.HasValue)
        {
            productsQuery =
                productsQuery.Where(product =>
                    product.Status == query.Status.Value
                );
        }

        int totalCount =
            await productsQuery.CountAsync(cancellationToken);

        int skipCount =
            checked((query.PageNumber - 1) * query.PageSize);

        List<ListProductItem> items =
            await productsQuery
            .OrderBy(product => product.Sku)
            .ThenBy(product => product.Id)
            .Skip(skipCount)
            .Take(query.PageSize)
            .Select(product =>
                new ListProductItem(
                    ProductId: product.Id,
                    Sku: product.Sku,
                    Name: product.Name,
                    UnitOfMeasureId: product.UnitOfMeasureId,
                    ProductCategoryId: product.ProductCategoryId,
                    TaxCategoryId: product.TaxCategoryId,
                    Status: product.Status,
                    CreatedAt: product.CreatedAt,
                    LastModifiedAt: product.LastModifiedAt))
            .ToListAsync(cancellationToken);
        int totalPages =
             totalCount == 0 ? 0 : ((totalCount - 1) / query.PageSize) + 1;
        return new ListProductsResult(
            Items: items,
            PageNumber: query.PageNumber,
            PageSize: query.PageSize,
            TotalCount: totalCount,
            TotalPages: totalPages
            );
    }
    private static void ValidateQuery(ListProductsQuery query)
    {
        if (query.OrganisationId == Guid.Empty)
        {
            throw new RequestValidationException(ListProductsErrors.OrganisationRequiredCode, ListProductsErrors.OrganisationRequiredMessage);
        }
        if (query.PageNumber < 1)
        {
            throw new RequestValidationException(ListProductsErrors.PageNumberInvalidCode, ListProductsErrors.PageNumberInvalidMessage);
        }
        if (query.PageSize < 1 || query.PageSize > ListProductsErrors.MaxPageSize)
        {
            throw new RequestValidationException(ListProductsErrors.PageSizeInvalidCode, ListProductsErrors.PageSizeInvalidMessage);
        }
    }

    private static string? NormalizeSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        string normalizedSearch = search.Trim();

        if (normalizedSearch.Length > ListProductsErrors.MaxSearchLength)
        {
            throw new RequestValidationException(ListProductsErrors.SearchInvalidCode, ListProductsErrors.SearchInvalidMessage);
        }


        return normalizedSearch;
    }
}
