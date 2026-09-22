using TradeFlow.Modules.Catalog.Domain.ProductCategories;

namespace TradeFlow.Api.Endpoints.Catalog.ProductCategories;

public sealed class ListProductCategoriesRequest
{
    public string? Search { get; init; }
    public ProductCategoryStatus? Status { get; init; }
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
}
