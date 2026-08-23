using TradeFlow.Modules.Catalog.Domain.Products;

namespace TradeFlow.Modules.Catalog.Application.Products.DeactivateProduct;

public sealed record DeactivateProductResult
(
    Guid ProductId,
    ProductStatus Status,
    DateTimeOffset? LastModifiedAt,
    Guid? LastModifiedBy,
    byte[] RowVersion
    );
