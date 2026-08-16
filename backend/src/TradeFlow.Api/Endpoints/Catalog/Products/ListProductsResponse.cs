namespace TradeFlow.Api.Endpoints.Catalog.Products;

public sealed record ListProductsResponse
(
    IReadOnlyList<ListProductItemResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages
    );
