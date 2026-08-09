namespace TradeFlow.Api.Endpoints.Catalog.Products;

public sealed record CreateProductResponse
(
    Guid ProductId,
    string Sku
    );
