using System;
using System.Text.RegularExpressions;
using TradeFlow.BuildingBlocks.Exceptions;

namespace TradeFlow.Modules.Catalog.Domain.ProductCategories;

public sealed class ProductCategory
{
    public Guid Id { get; private set; }
    public Guid OrganisationId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ProductCategoryStatus Status { get; private set; } = ProductCategoryStatus.Active;
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedAt { get; private set; }
    public Guid? LastModifiedBy { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
    internal const int MaxCodeLength = 50;
    internal const int MaxNameLength = 100;
    internal const int MaxDescriptionLength = 1000;

    public static ProductCategory Create(Guid organisationId, string code, string name, string? description, DateTimeOffset createdAt, Guid createdBy)
    {
        ValidateRequiredIdentifiers(organisationId, createdBy, createdAt);
        return new ProductCategory
        {
            Id = Guid.NewGuid(),
            OrganisationId = organisationId,
            Code = NormalizeCode(code),
            Name = NormalizeName(name),
            Description = NormalizeDescription(description),
            Status = ProductCategoryStatus.Active,
            CreatedAt = createdAt.ToUniversalTime(),
            CreatedBy = createdBy,
            LastModifiedAt = null,
            LastModifiedBy = null,
            RowVersion = Array.Empty<byte>()
        };
    }
    private static void ValidateRequiredIdentifiers(Guid organisationId, Guid createdBy, DateTimeOffset createdAt)
    {
        EnsureNotEmpty(organisationId, ProductCategoryErrors.OrganisationRequiredCode, ProductCategoryErrors.OrganisationRequiredMessage);
        EnsureNotEmpty(createdBy, ProductCategoryErrors.CreatedByRequiredCode, ProductCategoryErrors.CreatedByRequiredMessage);
        if (createdAt == default)
        {
            throw new BusinessRuleException(ProductCategoryErrors.CreatedAtRequiredCode, ProductCategoryErrors.CreatedAtRequiredMessage);
        }

    }

    private static void ValidateModificationAudit(DateTimeOffset modifiedAt, Guid modifiedBy)
    {
        EnsureNotEmpty(modifiedBy, ProductCategoryErrors.ModifiedByRequiredCode, ProductCategoryErrors.ModifiedByRequiredMessage);

        if (modifiedAt == default)
        {
            throw new BusinessRuleException(ProductCategoryErrors.ModifiedAtRequiredCode, ProductCategoryErrors.ModifiedAtRequiredMessage);
        }
    }

    private static void EnsureNotEmpty(Guid value, string errorCode, string errorMessage)
    {
        if (value == Guid.Empty)
        {
            throw new BusinessRuleException(errorCode, errorMessage);
        }

    }

    public void Activate(DateTimeOffset modifiedAt, Guid modifiedBy)
    {
        ValidateModificationAudit(modifiedAt, modifiedBy);
        if (Status == ProductCategoryStatus.Active)
        {
            return;
        }
        Status = ProductCategoryStatus.Active;
        LastModifiedAt = modifiedAt.ToUniversalTime();
        LastModifiedBy = modifiedBy;
    }

    public void Deactivate(DateTimeOffset modifiedAt, Guid modifiedBy)
    {
        ValidateModificationAudit(modifiedAt, modifiedBy);
        if (Status == ProductCategoryStatus.Inactive)
        {
            return;
        }
        Status = ProductCategoryStatus.Inactive;
        LastModifiedAt = modifiedAt.ToUniversalTime();
        LastModifiedBy = modifiedBy;
    }

    public void UpdateDetails(string name, string? description, DateTimeOffset modifiedAt, Guid modifiedBy)
    {
        ValidateModificationAudit(modifiedAt, modifiedBy);
        string normalizedName = NormalizeName(name);
        string normalizedDescription = NormalizeDescription(description);
        bool hasNoChanges =
            Name == normalizedName && Description == normalizedDescription;
        if (hasNoChanges) {
            return;
        }
        Name = normalizedName;
        Description = normalizedDescription;
        LastModifiedAt = modifiedAt.ToUniversalTime();
        LastModifiedBy = modifiedBy;

    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new BusinessRuleException(ProductCategoryErrors.CodeRequiredCode, ProductCategoryErrors.CodeRequiredMessage);
        }
        string normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Length > MaxCodeLength)
        {
            throw new BusinessRuleException(ProductCategoryErrors.CodeInvalidCode, ProductCategoryErrors.CodeOverLengthMessage);
        }
        if (normalizedCode.Any(char.IsWhiteSpace))
        {
            throw new BusinessRuleException(ProductCategoryErrors.CodeInvalidCode, ProductCategoryErrors.CodeInvalidMessage);
        }
        if (!normalizedCode.All(x => ((x >= 'A' && x <= 'Z') || (x >= '0' && x <= '9') || x == '-')))
        {
            throw new BusinessRuleException(ProductCategoryErrors.CodeInvalidCode, ProductCategoryErrors.CodeInvalidCharactersMessage);
        }
        return normalizedCode;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException(ProductCategoryErrors.NameRequiredCode, ProductCategoryErrors.NameRequiredMessage);
        }
        string normalizedName = Regex.Replace(name.Trim(), @"\s+", " ");
        if (normalizedName.Length > MaxNameLength)
        {
            throw new BusinessRuleException(ProductCategoryErrors.NameInvalidCode, ProductCategoryErrors.NameOverLengthMessage);
        }
        return normalizedName;

    }

    private static string NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return string.Empty;
        }
        string normalizedDescription = description.Trim();

        if (normalizedDescription.Length > MaxDescriptionLength)
        {
            throw new BusinessRuleException(ProductCategoryErrors.DescriptionInvalidCode, ProductCategoryErrors.DescriptionOverLengthMessage);
        }
        return normalizedDescription;
    }
    private ProductCategory()
    {
    }
}
