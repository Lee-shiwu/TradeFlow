namespace TradeFlow.Modules.Catalog.Application.TaxCategories.ListTaxCategories;

public sealed record ListTaxCategoriesResult(
    IReadOnlyList<ListTaxCategoryItem> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
