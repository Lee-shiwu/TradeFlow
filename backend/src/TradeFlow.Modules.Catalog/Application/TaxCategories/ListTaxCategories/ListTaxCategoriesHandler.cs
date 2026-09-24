using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.Modules.Catalog.Application.TaxCategories.ListTaxCategories;

public sealed class ListTaxCategoriesHandler(CatalogDbContext dbContext)
{
    public async Task<ListTaxCategoriesResult> HandleAsync(
        ListTaxCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        ValidateQuery(query);

        string? normalizedSearch = NormalizeSearch(query.Search);
        IQueryable<TaxCategory> taxCategoriesQuery =
            dbContext.TaxCategories.AsNoTracking();

        if (normalizedSearch is not null)
        {
            taxCategoriesQuery = taxCategoriesQuery.Where(category =>
                category.Code.Contains(normalizedSearch)
                || category.Name.Contains(normalizedSearch));
        }

        if (query.Status.HasValue)
        {
            taxCategoriesQuery = taxCategoriesQuery.Where(category =>
                category.Status == query.Status.Value);
        }

        int totalCount =
            await taxCategoriesQuery.CountAsync(cancellationToken);
        int skipCount = checked((query.PageNumber - 1) * query.PageSize);

        List<ListTaxCategoryItem> items =
            await taxCategoriesQuery
                .OrderBy(category => category.Code)
                .ThenBy(category => category.Id)
                .Skip(skipCount)
                .Take(query.PageSize)
                .Select(category =>
                    new ListTaxCategoryItem(
                        category.Id,
                        category.Code,
                        category.Name,
                        category.Description,
                        category.Rate,
                        category.Treatment,
                        category.Status,
                        category.EffectiveFrom,
                        category.EffectiveTo))
                .ToListAsync(cancellationToken);

        int totalPages =
            totalCount == 0
                ? 0
                : ((totalCount - 1) / query.PageSize) + 1;

        return new ListTaxCategoriesResult(
            items,
            query.PageNumber,
            query.PageSize,
            totalCount,
            totalPages);
    }

    private static void ValidateQuery(ListTaxCategoriesQuery query)
    {
        if (query.PageNumber < 1)
        {
            throw new RequestValidationException(
                ListTaxCategoriesErrors.PageNumberInvalidCode,
                ListTaxCategoriesErrors.PageNumberInvalidMessage);
        }

        if (query.PageSize < 1
            || query.PageSize > ListTaxCategoriesErrors.MaxPageSize)
        {
            throw new RequestValidationException(
                ListTaxCategoriesErrors.PageSizeInvalidCode,
                ListTaxCategoriesErrors.PageSizeInvalidMessage);
        }
    }

    private static string? NormalizeSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        string normalizedSearch = search.Trim();

        if (normalizedSearch.Length > ListTaxCategoriesErrors.MaxSearchLength)
        {
            throw new RequestValidationException(
                ListTaxCategoriesErrors.SearchInvalidCode,
                ListTaxCategoriesErrors.SearchInvalidMessage);
        }

        return normalizedSearch;
    }
}
