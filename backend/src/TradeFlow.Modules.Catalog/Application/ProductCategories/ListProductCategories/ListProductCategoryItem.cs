using TradeFlow.Modules.Catalog.Domain.ProductCategories;

namespace TradeFlow.Modules.Catalog.Application.ProductCategories.ListProductCategories;

public sealed record ListProductCategoryItem
(
    Guid ProductCategoryId,
    string Code,
    string Name,
    string Description,
    ProductCategoryStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastModifiedAt
);
