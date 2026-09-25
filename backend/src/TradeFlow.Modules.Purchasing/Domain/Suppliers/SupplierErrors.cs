namespace TradeFlow.Modules.Purchasing.Domain.Suppliers;

internal static class SupplierErrors
{
    public const string OrganisationRequiredCode =
        "SUPPLIER_ORGANISATION_REQUIRED";
    public const string OrganisationRequiredMessage =
        "Organisation is required.";

    public const string CodeRequiredCode = "SUPPLIER_CODE_REQUIRED";
    public const string CodeRequiredMessage = "Supplier code is required.";
    public const string CodeInvalidCode = "SUPPLIER_CODE_INVALID";
    public const string CodeInvalidMessage =
        "Supplier code can only contain letters, numbers, and hyphens.";
    public const string CodeOverLengthMessage =
        "Supplier code must not exceed 50 characters.";

    public const string NameRequiredCode = "SUPPLIER_NAME_REQUIRED";
    public const string NameRequiredMessage = "Supplier name is required.";
    public const string NameInvalidCode = "SUPPLIER_NAME_INVALID";
    public const string NameOverLengthMessage =
        "Supplier name must not exceed 150 characters.";

    public const string CreatedByRequiredCode =
        "SUPPLIER_CREATED_BY_REQUIRED";
    public const string CreatedByRequiredMessage = "Created by is required.";
}
