using System;
using System.Collections.Generic;
using System.Text;

namespace TradeFlow.Modules.Catalog.Domain.ProductCategories;

internal static class ProductCategoryErrors
{
    public const string NameRequiredCode =
        "PRODUCT_CATEGORY_NAME_REQUIRED";

    public const string NameRequiredMessage =
        "Product category name is required.";

    public const string NameInvalidCode =
        "PRODUCT_CATEGORY_NAME_INVALID";

    public const string NameOverLengthMessage =
        "Product category name must not exceed 100 characters.";

    public const string DescriptionInvalidCode =
        "PRODUCT_CATEGORY_DESCRIPTION_INVALID";

    public const string DescriptionOverLengthMessage =
        "Product category description must not exceed 1000 characters.";

    public const string CodeRequiredCode =
       "PRODUCT_CATEGORY_CODE_REQUIRED";

    public const string CodeRequiredMessage =
        "Product category code is required.";

    public const string CodeInvalidCode =
        "PRODUCT_CATEGORY_CODE_INVALID";

    public const string CodeInvalidMessage =
        "Product category code must not contain whitespace.";

    public const string CodeInvalidCharactersMessage =
        "Product category code can only consist of numbers, uppercase letters, and hyphens.";

    public const string CodeOverLengthMessage =
        "Product category code must not exceed 50 characters.";

    public const string OrganisationRequiredCode =
       "PRODUCT_CATEGORY_ORGANISATION_REQUIRED";

    public const string OrganisationRequiredMessage =
        "Product category organisation is required.";

    public const string CreatedByRequiredCode =
       "PRODUCT_CATEGORY_CREATED_BY_REQUIRED";

    public const string CreatedByRequiredMessage =
        "Product category creator is required.";

    public const string CreatedAtRequiredCode =
        "PRODUCT_CATEGORY_CREATED_AT_REQUIRED";

    public const string CreatedAtRequiredMessage =
        "Product category creation time is required.";

    public const string ModifiedByRequiredCode =
       "PRODUCT_CATEGORY_MODIFIED_BY_REQUIRED";

    public const string ModifiedByRequiredMessage =
        "Product category modifier is required.";

    public const string ModifiedAtRequiredCode =
        "PRODUCT_CATEGORY_MODIFIED_AT_REQUIRED";

    public const string ModifiedAtRequiredMessage =
        "Product category modification time is required.";
}
