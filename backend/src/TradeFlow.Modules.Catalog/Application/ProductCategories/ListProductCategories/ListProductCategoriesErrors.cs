namespace TradeFlow.Modules.Catalog.Application.ProductCategories.ListProductCategories;

internal static class ListProductCategoriesErrors
{
    internal const int MaxSearchLength = 100;
    internal const int MaxPageSize = 100;

    public const string OrganisationRequiredCode =
        "LIST_PRODUCT_CATEGORIES_ORGANISATION_REQUIRED";

    public const string OrganisationRequiredMessage =
        "Organisation is required.";

    public const string PageNumberInvalidCode =
        "LIST_PRODUCT_CATEGORIES_PAGE_NUMBER_INVALID";

    public const string PageNumberInvalidMessage =
        "Page number must be greater than or equal to 1.";

    public const string PageSizeInvalidCode =
        "LIST_PRODUCT_CATEGORIES_PAGE_SIZE_INVALID";

    public const string PageSizeInvalidMessage =
        "Page size must be between 1 and 100.";

    public const string SearchInvalidCode =
        "LIST_PRODUCT_CATEGORIES_SEARCH_INVALID";

    public const string SearchInvalidMessage =
        "Search must not exceed 100 characters.";
}
