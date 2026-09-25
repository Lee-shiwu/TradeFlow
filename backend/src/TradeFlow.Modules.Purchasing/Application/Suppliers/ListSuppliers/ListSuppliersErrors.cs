namespace TradeFlow.Modules.Purchasing.Application.Suppliers.ListSuppliers;

internal static class ListSuppliersErrors
{
    public const int MaxSearchLength = 150;
    public const int MaxPageSize = 100;

    public const string OrganisationRequiredCode =
        "LIST_SUPPLIERS_ORGANISATION_REQUIRED";
    public const string OrganisationRequiredMessage =
        "Organisation is required.";
    public const string PageNumberInvalidCode =
        "LIST_SUPPLIERS_PAGE_NUMBER_INVALID";
    public const string PageNumberInvalidMessage =
        "Page number must be greater than or equal to 1.";
    public const string PageSizeInvalidCode =
        "LIST_SUPPLIERS_PAGE_SIZE_INVALID";
    public const string PageSizeInvalidMessage =
        "Page size must be between 1 and 100.";
    public const string SearchInvalidCode =
        "LIST_SUPPLIERS_SEARCH_INVALID";
    public const string SearchInvalidMessage =
        "Search must not exceed 150 characters.";
}
