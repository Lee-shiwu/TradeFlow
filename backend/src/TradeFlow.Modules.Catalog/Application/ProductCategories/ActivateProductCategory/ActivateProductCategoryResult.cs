using TradeFlow.Modules.Catalog.Domain.ProductCategories;

namespace TradeFlow.Modules.Catalog.Application.ProductCategories.ActivateProductCategory;

public sealed record ActivateProductCategoryResult(
    Guid ProductCategoryId,
    ProductCategoryStatus Status,
    DateTimeOffset? LastModifiedAt,
    Guid? LastModifiedBy,
    byte[] RowVersion);
