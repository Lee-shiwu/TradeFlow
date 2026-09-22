using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.Modules.Catalog.Application.ProductCategories.ListProductCategories;

public sealed class ListProductCategoriesHandler(CatalogDbContext dbContext)
{
    public async Task<ListProductCategoriesResult> HandleAsync(
        ListProductCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        ValidateQuery(query);

        string? normalizedSearch = NormalizeSearch(query.Search);

        IQueryable<ProductCategory> categoriesQuery =
            dbContext.ProductCategories
                .AsNoTracking()
                .Where(category =>
                    category.OrganisationId == query.OrganisationId);

        if (normalizedSearch is not null)
        {
            categoriesQuery =
                categoriesQuery.Where(category =>
                    category.Code.Contains(normalizedSearch)
                    || category.Name.Contains(normalizedSearch));
        }

        if (query.Status.HasValue)
        {
            categoriesQuery =
                categoriesQuery.Where(category =>
                    category.Status == query.Status.Value);
        }

        int totalCount =
            await categoriesQuery.CountAsync(cancellationToken);

        int skipCount =
            checked((query.PageNumber - 1) * query.PageSize);

        List<ListProductCategoryItem> items =
            await categoriesQuery
                .OrderBy(category => category.Code)
                .ThenBy(category => category.Id)
                .Skip(skipCount)
                .Take(query.PageSize)
                .Select(category =>
                    new ListProductCategoryItem(
                        ProductCategoryId: category.Id,
                        Code: category.Code,
                        Name: category.Name,
                        Description: category.Description,
                        Status: category.Status,
                        CreatedAt: category.CreatedAt,
                        LastModifiedAt: category.LastModifiedAt))
                .ToListAsync(cancellationToken);

        int totalPages =
            totalCount == 0
                ? 0
                : ((totalCount - 1) / query.PageSize) + 1;

        return new ListProductCategoriesResult(
            Items: items,
            PageNumber: query.PageNumber,
            PageSize: query.PageSize,
            TotalCount: totalCount,
            TotalPages: totalPages);
    }

    private static void ValidateQuery(
        ListProductCategoriesQuery query)
    {
        if (query.OrganisationId == Guid.Empty)
        {
            throw new RequestValidationException(
                ListProductCategoriesErrors.OrganisationRequiredCode,
                ListProductCategoriesErrors.OrganisationRequiredMessage);
        }

        if (query.PageNumber < 1)
        {
            throw new RequestValidationException(
                ListProductCategoriesErrors.PageNumberInvalidCode,
                ListProductCategoriesErrors.PageNumberInvalidMessage);
        }

        if (query.PageSize < 1
            || query.PageSize > ListProductCategoriesErrors.MaxPageSize)
        {
            throw new RequestValidationException(
                ListProductCategoriesErrors.PageSizeInvalidCode,
                ListProductCategoriesErrors.PageSizeInvalidMessage);
        }
    }

    private static string? NormalizeSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        string normalizedSearch = search.Trim();

        if (normalizedSearch.Length
            > ListProductCategoriesErrors.MaxSearchLength)
        {
            throw new RequestValidationException(
                ListProductCategoriesErrors.SearchInvalidCode,
                ListProductCategoriesErrors.SearchInvalidMessage);
        }

        return normalizedSearch;
    }
}
