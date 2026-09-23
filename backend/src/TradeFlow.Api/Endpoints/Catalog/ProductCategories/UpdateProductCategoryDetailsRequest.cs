namespace TradeFlow.Api.Endpoints.Catalog.ProductCategories;

public sealed record UpdateProductCategoryDetailsRequest
(
    string Name,
    string? Description,
    string? RowVersion
);
