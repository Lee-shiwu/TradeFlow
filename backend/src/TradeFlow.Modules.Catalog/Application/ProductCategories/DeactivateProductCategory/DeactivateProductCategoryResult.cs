using TradeFlow.Modules.Catalog.Domain.ProductCategories;

namespace TradeFlow.Modules.Catalog.Application.ProductCategories.DeactivateProductCategory;

public sealed record DeactivateProductCategoryResult(
    Guid ProductCategoryId,
    ProductCategoryStatus Status,
    DateTimeOffset? LastModifiedAt,
    Guid? LastModifiedBy,
    byte[] RowVersion);
