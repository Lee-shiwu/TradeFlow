
using TradeFlow.Modules.Catalog.Domain.Products;
namespace TradeFlow.Modules.Catalog.Application.Products.GetProductById;

public sealed record GetProductByIdResult
(
    Guid ProductId,
    string Sku,
    string Name,
    string Description,
    Guid UnitOfMeasureId,
    Guid? ProductCategoryId,
    Guid TaxCategoryId,
    ProductStatus Status,
    DateTimeOffset CreatedAt,
    Guid CreatedBy,
    DateTimeOffset? LastModifiedAt,
    Guid? LastModifiedBy,
    byte[] RowVersion
    );
