namespace TradeFlow.Modules.Catalog.Application.Products.CreateProduct;

internal static class CreateProductErrors
{
    public const string SkuAlreadyExistsCode =
        "PRODUCT_SKU_ALREADY_EXISTS";

    public const string SkuAlreadyExistsMessage =
        "A product with this SKU already exists in the organisation.";

    public const string UnitOfMeasureNotFoundCode =
        "PRODUCT_UNIT_OF_MEASURE_NOT_FOUND";

    public const string UnitOfMeasureNotFoundMessage =
        "The selected unit of measure does not exist.";

    public const string UnitOfMeasureInactiveCode =
        "PRODUCT_UNIT_OF_MEASURE_INACTIVE";

    public const string UnitOfMeasureInactiveMessage =
        "The selected unit of measure is inactive.";

    public const string ProductCategoryNotFoundCode =
        "PRODUCT_CATEGORY_NOT_FOUND";

    public const string ProductCategoryNotFoundMessage =
        "The selected product category does not exist in the organisation.";

    public const string ProductCategoryInactiveCode =
        "PRODUCT_CATEGORY_INACTIVE";

    public const string ProductCategoryInactiveMessage =
        "The selected product category is inactive.";

    public const string TaxCategoryNotFoundCode =
        "PRODUCT_TAX_CATEGORY_NOT_FOUND";

    public const string TaxCategoryNotFoundMessage =
        "The selected tax category does not exist.";

    public const string TaxCategoryInactiveCode =
        "PRODUCT_TAX_CATEGORY_INACTIVE";

    public const string TaxCategoryInactiveMessage =
        "The selected tax category is inactive.";

    public const string TaxCategoryNotEffectiveCode =
        "PRODUCT_TAX_CATEGORY_NOT_EFFECTIVE";

    public const string TaxCategoryNotEffectiveMessage =
        "The selected tax category is not effective on the product creation date.";
}
