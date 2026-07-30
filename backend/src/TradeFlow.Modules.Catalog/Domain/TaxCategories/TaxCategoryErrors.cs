using System;
using System.Collections.Generic;
using System.Text;

namespace TradeFlow.Modules.Catalog.Domain.TaxCategories;

internal static class TaxCategoryErrors
{
    public const string CodeRequiredCode =
      "TAX_CATEGORY_CODE_REQUIRED";

    public const string CodeRequiredMessage =
        "Tax category code is required.";

    public const string CodeInvalidCode =
        "TAX_CATEGORY_CODE_INVALID";

    public const string CodeInvalidMessage =
        "Tax category code must not contain whitespace.";

    public const string CodeInvalidCharactersMessage =
        "Tax category code can only consist of numbers, uppercase letters, and hyphens.";

    public const string CodeOverLengthMessage =
        "Tax category code must not exceed 30 characters.";

    public const string NameRequiredCode =
       "TAX_CATEGORY_NAME_REQUIRED";

    public const string NameRequiredMessage =
        "Tax category name is required.";

    public const string NameInvalidCode =
        "TAX_CATEGORY_NAME_INVALID";

    public const string NameOverLengthMessage =
        "Tax category name must not exceed 100 characters.";

    public const string DescriptionInvalidCode =
        "TAX_CATEGORY_DESCRIPTION_INVALID";

    public const string DescriptionOverLengthMessage =
        "Tax category description must not exceed 500 characters.";

    public const string RateInvalidCode =
        "TAX_CATEGORY_RATE_INVALID";

    public const string RateRangeMessage =
        "Tax category rate must be between 0 and 1.";

    public const string RateScaleMessage =
        "Tax category rate must not exceed 4 decimal places.";

    public const string TreatmentInvalidCode =
        "TAX_CATEGORY_TREATMENT_INVALID";

    public const string TreatmentInvalidMessage =
        "Tax category treatment is invalid.";

    public const string TreatmentRateMismatchCode =
        "TAX_CATEGORY_TREATMENT_RATE_MISMATCH";

    public const string TreatmentRateMismatchMessage =
        "Tax category rate does not match its treatment.";

    public const string EffectiveFromRequiredCode =
        "TAX_CATEGORY_EFFECTIVE_FROM_REQUIRED";

    public const string EffectiveFromRequiredMessage =
        "Tax category effective-from date is required.";

    public const string EffectivePeriodInvalidCode =
        "TAX_CATEGORY_EFFECTIVE_PERIOD_INVALID";

    public const string EffectivePeriodInvalidMessage =
        "Tax category effective-to date cannot be earlier than the effective-from date.";
}
