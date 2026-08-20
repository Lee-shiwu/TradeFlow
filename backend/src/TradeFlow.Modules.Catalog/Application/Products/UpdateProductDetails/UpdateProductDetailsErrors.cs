namespace TradeFlow.Modules.Catalog.Application.Products.UpdateProductDetails;

internal static class UpdateProductDetailsErrors
{
    public const string OrganisationRequiredCode =
        "UPDATE_PRODUCT_ORGANISATION_REQUIRED";

    public const string OrganisationRequiredMessage =
        "Organisation is required.";

    public const string ProductIdRequiredCode =
        "UPDATE_PRODUCT_ID_REQUIRED";

    public const string ProductIdRequiredMessage =
        "Product ID is required.";

    public const string ModifiedByRequiredCode =
        "UPDATE_PRODUCT_MODIFIED_BY_REQUIRED";

    public const string ModifiedByRequiredMessage =
        "Modified by is required.";

    public const string RowVersionRequiredCode =
        "UPDATE_PRODUCT_ROW_VERSION_REQUIRED";

    public const string RowVersionRequiredMessage =
        "Product row version is required.";

    public const string ProductNotFoundCode =
        "UPDATE_PRODUCT_NOT_FOUND";

    public const string ProductNotFoundMessage =
        "The selected product does not exist.";

    public const string ProductCategoryNotFoundCode =
        "UPDATE_PRODUCT_CATEGORY_NOT_FOUND";

    public const string ProductCategoryNotFoundMessage =
        "The selected product category does not exist.";

    public const string ProductCategoryInactiveCode =
        "UPDATE_PRODUCT_CATEGORY_INACTIVE";

    public const string ProductCategoryInactiveMessage =
        "The selected product category is inactive.";

    public const string TaxCategoryRequiredCode =
        "UPDATE_PRODUCT_TAX_CATEGORY_REQUIRED";

    public const string TaxCategoryRequiredMessage =
        "Tax category is required.";

    public const string TaxCategoryNotFoundCode =
        "UPDATE_PRODUCT_TAX_CATEGORY_NOT_FOUND";

    public const string TaxCategoryNotFoundMessage =
        "The selected tax category does not exist.";

    public const string TaxCategoryInactiveCode =
        "UPDATE_PRODUCT_TAX_CATEGORY_INACTIVE";

    public const string TaxCategoryInactiveMessage =
        "The selected tax category is inactive.";

    public const string TaxCategoryNotEffectiveCode =
        "UPDATE_PRODUCT_TAX_CATEGORY_NOT_EFFECTIVE";

    public const string TaxCategoryNotEffectiveMessage =
        "The selected tax category is not effective on the current date.";

    public const string ConcurrencyConflictCode =
        "UPDATE_PRODUCT_CONCURRENCY_CONFLICT";

    public const string ConcurrencyConflictMessage =
        "The product was modified by another user. Reload the product and try again.";
}
