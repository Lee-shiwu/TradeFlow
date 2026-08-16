using TradeFlow.Modules.Catalog.Domain.Products;

namespace TradeFlow.Modules.Catalog.Application.Products.ListProducts;

public sealed record ListProductItem
(
    Guid ProductId,
    string Sku,
    string Name,
    Guid UnitOfMeasureId,
    Guid? ProductCategoryId,
    Guid TaxCategoryId,
    ProductStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastModifiedAt
    );
