namespace TradeFlow.Api.Endpoints.Catalog.TaxCategories;

public sealed record ListTaxCategoriesResponse(
    IReadOnlyList<ListTaxCategoryItemResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
