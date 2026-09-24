using TradeFlow.Modules.Catalog.Domain.TaxCategories;

namespace TradeFlow.Api.Endpoints.Catalog.TaxCategories;

public sealed record ListTaxCategoriesRequest
{
    public string? Search { get; init; }
    public TaxCategoryStatus? Status { get; init; }
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
}
