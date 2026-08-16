namespace TradeFlow.Modules.Catalog.Application.Products.ListProducts;

public sealed record ListProductsResult
(
    IReadOnlyList<ListProductItem> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages
    );
