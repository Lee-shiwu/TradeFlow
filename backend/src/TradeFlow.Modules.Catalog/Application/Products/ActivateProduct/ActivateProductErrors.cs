namespace TradeFlow.Modules.Catalog.Application.Products.ActivateProduct;

internal static class ActivateProductErrors
{
    public const string OrganisationRequiredCode =
       "ACTIVATE_PRODUCT_ORGANISATION_REQUIRED";

    public const string OrganisationRequiredMessage =
        "Organisation is required.";

    public const string ProductIdRequiredCode =
        "ACTIVATE_PRODUCT_ID_REQUIRED";

    public const string ProductIdRequiredMessage =
        "Product ID is required.";

    public const string ModifiedByRequiredCode =
        "ACTIVATE_PRODUCT_MODIFIED_BY_REQUIRED";

    public const string ModifiedByRequiredMessage =
        "Modified by is required.";

    public const string RowVersionRequiredCode =
        "ACTIVATE_PRODUCT_ROW_VERSION_REQUIRED";

    public const string RowVersionRequiredMessage =
        "Product row version is required.";

    public const string ProductNotFoundCode =
        "ACTIVATE_PRODUCT_NOT_FOUND";

    public const string ProductNotFoundMessage =
        "The selected product does not exist.";

    public const string ConcurrencyConflictCode =
        "ACTIVATE_PRODUCT_CONCURRENCY_CONFLICT";

    public const string ConcurrencyConflictMessage =
        "The product was modified by another user. Reload the product and try again.";

    public const string ProductCategoryNotFoundCode =
       "ACTIVATE_PRODUCT_CATEGORY_NOT_FOUND";

    public const string ProductCategoryNotFoundMessage =
        "The selected product category does not exist.";

    public const string ProductCategoryInactiveCode =
        "ACTIVATE_PRODUCT_CATEGORY_INACTIVE";

    public const string ProductCategoryInactiveMessage =
        "The selected product category is inactive.";

    public const string TaxCategoryNotFoundCode =
        "ACTIVATE_PRODUCT_TAX_CATEGORY_NOT_FOUND";

    public const string TaxCategoryNotFoundMessage =
        "The selected tax category does not exist.";

    public const string TaxCategoryInactiveCode =
        "ACTIVATE_PRODUCT_TAX_CATEGORY_INACTIVE";

    public const string TaxCategoryInactiveMessage =
        "The selected tax category is inactive.";

    public const string TaxCategoryNotEffectiveCode =
        "ACTIVATE_PRODUCT_TAX_CATEGORY_NOT_EFFECTIVE";

    public const string TaxCategoryNotEffectiveMessage =
        "The selected tax category is not effective on the current date.";

    public const string UnitOfMeasureNotFoundCode =
       "ACTIVATE_PRODUCT_UNIT_OF_MEASURE_NOT_FOUND";

    public const string UnitOfMeasureNotFoundMessage =
        "The selected unit of measure does not exist.";

    public const string UnitOfMeasureInactiveCode =
        "ACTIVATE_PRODUCT_UNIT_OF_MEASURE_INACTIVE";

    public const string UnitOfMeasureInactiveMessage =
        "The selected unit of measure is inactive.";
}
