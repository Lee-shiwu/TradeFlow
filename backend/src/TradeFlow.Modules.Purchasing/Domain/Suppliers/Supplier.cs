using System.Text.RegularExpressions;
using TradeFlow.BuildingBlocks.Exceptions;

namespace TradeFlow.Modules.Purchasing.Domain.Suppliers;

public sealed class Supplier
{
    internal const int MaxCodeLength = 50;
    internal const int MaxNameLength = 150;

    public Guid Id { get; private set; }
    public Guid OrganisationId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public SupplierStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public static Supplier Create(
        Guid organisationId,
        string code,
        string name,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        if (organisationId == Guid.Empty)
        {
            throw new BusinessRuleException(
                SupplierErrors.OrganisationRequiredCode,
                SupplierErrors.OrganisationRequiredMessage);
        }

        if (createdBy == Guid.Empty)
        {
            throw new BusinessRuleException(
                SupplierErrors.CreatedByRequiredCode,
                SupplierErrors.CreatedByRequiredMessage);
        }

        return new Supplier
        {
            Id = Guid.NewGuid(),
            OrganisationId = organisationId,
            Code = NormalizeCode(code),
            Name = NormalizeName(name),
            Status = SupplierStatus.Active,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
            RowVersion = []
        };
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new BusinessRuleException(
                SupplierErrors.CodeRequiredCode,
                SupplierErrors.CodeRequiredMessage);
        }

        string normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Length > MaxCodeLength)
        {
            throw new BusinessRuleException(
                SupplierErrors.CodeInvalidCode,
                SupplierErrors.CodeOverLengthMessage);
        }

        if (!normalizedCode.All(character =>
                character is >= 'A' and <= 'Z'
                || character is >= '0' and <= '9'
                || character == '-'))
        {
            throw new BusinessRuleException(
                SupplierErrors.CodeInvalidCode,
                SupplierErrors.CodeInvalidMessage);
        }

        return normalizedCode;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException(
                SupplierErrors.NameRequiredCode,
                SupplierErrors.NameRequiredMessage);
        }

        string normalizedName = Regex.Replace(name.Trim(), @"\s+", " ");

        if (normalizedName.Length > MaxNameLength)
        {
            throw new BusinessRuleException(
                SupplierErrors.NameInvalidCode,
                SupplierErrors.NameOverLengthMessage);
        }

        return normalizedName;
    }

    private Supplier()
    {
    }
}
