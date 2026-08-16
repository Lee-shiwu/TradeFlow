namespace TradeFlow.Modules.Catalog.Application.Products.ListProducts;

internal static class ListProductsErrors
{
    internal const int MaxSearchLength = 100;

    internal const int MaxPageSize = 100;

    public const string OrganisationRequiredCode =
        "LIST_PRODUCTS_ORGANISATION_REQUIRED";

    public const string OrganisationRequiredMessage =
        "Organisation is required.";

    public const string PageNumberInvalidCode =
        "LIST_PRODUCTS_PAGE_NUMBER_INVALID";

    public const string PageNumberInvalidMessage =
        "Page number must be greater than or equal to 1.";

    public const string PageSizeInvalidCode =
        "LIST_PRODUCTS_PAGE_SIZE_INVALID";

    public const string PageSizeInvalidMessage =
        "Page size must be between 1 and 100.";

    public const string SearchInvalidCode =
        "LIST_PRODUCTS_SEARCH_INVALID";

    public const string SearchInvalidMessage =
        "Search must not exceed 100 characters.";
}
