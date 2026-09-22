using TradeFlow.Modules.Catalog.Domain.ProductCategories;

namespace TradeFlow.Modules.Catalog.Application.ProductCategories.ListProductCategories;

public sealed record ListProductCategoriesQuery
(
    Guid OrganisationId,
    string? Search,
    ProductCategoryStatus? Status,
    int PageNumber,
    int PageSize
);
