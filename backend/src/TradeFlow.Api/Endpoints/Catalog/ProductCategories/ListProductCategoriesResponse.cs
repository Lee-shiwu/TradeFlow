namespace TradeFlow.Api.Endpoints.Catalog.ProductCategories;

public sealed record ListProductCategoriesResponse
(
    IReadOnlyList<ListProductCategoryItemResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages
);
