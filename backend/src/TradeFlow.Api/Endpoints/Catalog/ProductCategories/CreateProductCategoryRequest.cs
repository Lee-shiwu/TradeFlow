namespace TradeFlow.Api.Endpoints.Catalog.ProductCategories;

public sealed record CreateProductCategoryRequest
(
    string Code,
    string Name,
    string? Description
);
