namespace TradeFlow.Api.Endpoints.Catalog.ProductCategories;

internal static class ProductCategoryStatusEndpointHelper
{
    public static bool TryDecodeRowVersion(
        string? rowVersion,
        out byte[] value)
    {
        value = Array.Empty<byte>();

        if (string.IsNullOrWhiteSpace(rowVersion))
        {
            return false;
        }

        try
        {
            value = Convert.FromBase64String(rowVersion);
        }
        catch (FormatException)
        {
            return false;
        }

        return value.Length > 0;
    }

    public static IResult CreateInvalidRowVersionResult()
    {
        Dictionary<string, string[]> errors =
            new()
            {
                ["RowVersion"] =
                [
                    "RowVersion must be a valid non-empty Base64 value."
                ]
            };

        return Results.ValidationProblem(errors);
    }
}
