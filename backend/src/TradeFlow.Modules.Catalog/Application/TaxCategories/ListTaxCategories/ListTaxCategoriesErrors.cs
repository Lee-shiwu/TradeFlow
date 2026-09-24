namespace TradeFlow.Modules.Catalog.Application.TaxCategories.ListTaxCategories;

internal static class ListTaxCategoriesErrors
{
    public const int MaxSearchLength = 100;
    public const int MaxPageSize = 100;

    public const string PageNumberInvalidCode =
        "LIST_TAX_CATEGORIES_PAGE_NUMBER_INVALID";
    public const string PageNumberInvalidMessage =
        "Page number must be greater than or equal to 1.";
    public const string PageSizeInvalidCode =
        "LIST_TAX_CATEGORIES_PAGE_SIZE_INVALID";
    public const string PageSizeInvalidMessage =
        "Page size must be between 1 and 100.";
    public const string SearchInvalidCode =
        "LIST_TAX_CATEGORIES_SEARCH_INVALID";
    public const string SearchInvalidMessage =
        "Search must not exceed 100 characters.";
}
