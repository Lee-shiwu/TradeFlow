namespace TradeFlow.Modules.Catalog.Application.Products.DeactivateProduct;

internal static class DeactivateProductErrors
{
    public const string OrganisationRequiredCode =
        "DEACTIVATE_PRODUCT_ORGANISATION_REQUIRED";

    public const string OrganisationRequiredMessage =
        "Organisation is required.";

    public const string ProductIdRequiredCode =
        "DEACTIVATE_PRODUCT_ID_REQUIRED";

    public const string ProductIdRequiredMessage =
        "Product ID is required.";

    public const string ModifiedByRequiredCode =
        "DEACTIVATE_PRODUCT_MODIFIED_BY_REQUIRED";

    public const string ModifiedByRequiredMessage =
        "Modified by is required.";

    public const string RowVersionRequiredCode =
        "DEACTIVATE_PRODUCT_ROW_VERSION_REQUIRED";

    public const string RowVersionRequiredMessage =
        "Product row version is required.";

    public const string ProductNotFoundCode =
        "DEACTIVATE_PRODUCT_NOT_FOUND";

    public const string ProductNotFoundMessage =
        "The selected product does not exist.";

    public const string ConcurrencyConflictCode =
        "DEACTIVATE_PRODUCT_CONCURRENCY_CONFLICT";

    public const string ConcurrencyConflictMessage =
        "The product was modified by another user. Reload the product and try again.";
}
