using System;
using System.Collections.Generic;
using System.Text;

namespace TradeFlow.Modules.Catalog.Domain.UnitsOfMeasure;

internal static  class UnitOfMeasureErrors
{
    public const string CodeRequiredCode =
       "UNIT_OF_MEASURE_CODE_REQUIRED";

    public const string CodeRequiredMessage =
        "Unit of measure code is required.";

    public const string CodeInvalidCode =
        "UNIT_OF_MEASURE_CODE_INVALID";

    public const string CodeTooLongMessage =
        "Unit of measure code must not exceed 10 characters.";

    public const string CodeInvalidCharactersMessage =
        "Unit of measure code can only contain letters from A to Z.";

    public const string CodeUnsupportedCode =
        "UNIT_OF_MEASURE_CODE_UNSUPPORTED";

    public const string CodeUnsupportedMessage =
        "Unit of measure code is not supported.";
}
