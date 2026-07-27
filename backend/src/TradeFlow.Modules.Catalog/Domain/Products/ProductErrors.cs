using System;
using System.Collections.Generic;
using System.Text;

namespace TradeFlow.Modules.Catalog.Domain.Products;

internal static class ProductErrors
{
    public const string SkuRequiredCode =
        "PRODUCT_SKU_REQUIRED";

    public const string SkuRequiredMessage =
        "Product SKU is required.";

    public const string SkuInvalidCode =
        "PRODUCT_SKU_INVALID";

    public const string SkuInvalidMessage =
        "Product SKU must not contain whitespace.";

    public const string SkuInvalidCharactersMessage =
        "Product SKU can only consist of numbers, uppercase letters, and hyphens.";

    public const string SkuOverLengthMessage =
        "Product SKU must not exceed 50 characters.";

    public const string NameRequiredCode =
        "PRODUCT_NAME_REQUIRED";

    public const string NameRequiredMessage =
        "Product name is required.";

    public const string NameInvalidCode =
        "PRODUCT_NAME_INVALID";

    public const string NameOverLengthMessage =
        "Product name must not exceed 100 characters.";

    public const string DescriptionInvalidCode =
        "PRODUCT_DESCRIPTION_INVALID";

    public const string DescriptionInvalidMessage =
        "Product description must not exceed 1000 characters.";

    public const string OrganisationRequiredCode =
        "PRODUCT_ORGANISATION_REQUIRED";

    public const string OrganisationRequiredMessage =
        "Product organisation is required.";

    public const string UnitOfMeasureRequiredCode =
        "PRODUCT_UNIT_OF_MEASURE_REQUIRED";

    public const string UnitOfMeasureRequiredMessage =
        "Product unit of measure is required.";

    public const string TaxCategoryRequiredCode =
        "PRODUCT_TAX_CATEGORY_REQUIRED";

    public const string TaxCategoryRequiredMessage =
        "Product tax category is required.";

    public const string CreatedByRequiredCode =
        "PRODUCT_CREATED_BY_REQUIRED";

    public const string CreatedByRequiredMessage =
        "Product creator is required.";

    public const string ProductCategoryInvalidCode =
        "PRODUCT_CATEGORY_INVALID";

    public const string ProductCategoryInvalidMessage =
        "Product category identifier is invalid.";

    public const string ModifiedByRequiredCode =
        "PRODUCT_MODIFIED_BY_REQUIRED";

    public const string ModifiedByRequiredMessage =
        "Product modifier is required.";

    public const string ModifiedAtInvalidCode =
    "PRODUCT_MODIFIED_AT_INVALID";

    public const string ModifiedAtInvalidMessage =
        "Product modification time is invalid.";
}
