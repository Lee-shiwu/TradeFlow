using TradeFlow.Modules.Catalog.Domain.ProductCategories;

namespace TradeFlow.Modules.Catalog.Application.ProductCategories.UpdateProductCategoryDetails;

public sealed record UpdateProductCategoryDetailsResult
(
    Guid ProductCategoryId,
    string Code,
    string Name,
    string Description,
    ProductCategoryStatus Status,
    DateTimeOffset CreatedAt,
    Guid CreatedBy,
    DateTimeOffset? LastModifiedAt,
    Guid? LastModifiedBy,
    byte[] RowVersion
);
