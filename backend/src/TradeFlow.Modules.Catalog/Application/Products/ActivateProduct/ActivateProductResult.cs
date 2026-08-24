using TradeFlow.Modules.Catalog.Domain.Products;
namespace TradeFlow.Modules.Catalog.Application.Products.ActivateProduct;

public sealed record ActivateProductResult
(
    Guid ProductId,
    ProductStatus Status,
    DateTimeOffset? LastModifiedAt,
    Guid? LastModifiedBy,
    byte[] RowVersion
);
