namespace TradeFlow.Modules.Catalog.Application.ProductCategories.ActivateProductCategory;

internal static class ActivateProductCategoryErrors
{
    public const string OrganisationRequiredCode =
        "ACTIVATE_PRODUCT_CATEGORY_ORGANISATION_REQUIRED";
    public const string OrganisationRequiredMessage =
        "Organisation is required.";
    public const string ProductCategoryIdRequiredCode =
        "ACTIVATE_PRODUCT_CATEGORY_ID_REQUIRED";
    public const string ProductCategoryIdRequiredMessage =
        "Product category ID is required.";
    public const string ModifiedByRequiredCode =
        "ACTIVATE_PRODUCT_CATEGORY_MODIFIED_BY_REQUIRED";
    public const string ModifiedByRequiredMessage =
        "Modified by is required.";
    public const string RowVersionRequiredCode =
        "ACTIVATE_PRODUCT_CATEGORY_ROW_VERSION_REQUIRED";
    public const string RowVersionRequiredMessage =
        "Product category row version is required.";
    public const string ProductCategoryNotFoundCode =
        "ACTIVATE_PRODUCT_CATEGORY_NOT_FOUND";
    public const string ProductCategoryNotFoundMessage =
        "The selected product category does not exist.";
    public const string ConcurrencyConflictCode =
        "ACTIVATE_PRODUCT_CATEGORY_CONCURRENCY_CONFLICT";
    public const string ConcurrencyConflictMessage =
        "The product category was modified by another request. Reload it and try again.";
}
