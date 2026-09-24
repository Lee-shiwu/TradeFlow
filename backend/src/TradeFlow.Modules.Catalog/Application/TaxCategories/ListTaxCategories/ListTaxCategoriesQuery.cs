using TradeFlow.Modules.Catalog.Domain.TaxCategories;

namespace TradeFlow.Modules.Catalog.Application.TaxCategories.ListTaxCategories;

public sealed record ListTaxCategoriesQuery(
    string? Search,
    TaxCategoryStatus? Status,
    int PageNumber,
    int PageSize);
