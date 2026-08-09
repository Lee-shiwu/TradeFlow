namespace TradeFlow.Modules.Catalog.Application.Products.CreateProduct;

public sealed record CreateProductResult(
    Guid ProductId,
    string Sku
    );
