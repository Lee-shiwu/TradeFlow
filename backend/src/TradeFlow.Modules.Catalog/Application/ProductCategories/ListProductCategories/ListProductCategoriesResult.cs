namespace TradeFlow.Modules.Catalog.Application.ProductCategories.ListProductCategories;

public sealed record ListProductCategoriesResult
(
    IReadOnlyList<ListProductCategoryItem> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages
);
