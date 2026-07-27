using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TradeFlow.BuildingBlocks.Exceptions;
using System.Linq;
using System.Text.RegularExpressions;

namespace TradeFlow.Modules.Catalog.Domain.Products;

public class Product
{
    public Guid Id { get; private set; }
    public Guid OrganisationId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid UnitOfMeasureId { get; private set; }
    public Guid? ProductCategoryId { get; private set; }
    public Guid TaxCategoryId { get; private set; }
    public ProductStatus Status { get; private set; } = ProductStatus.Active;
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedAt { get; private set; }
    public Guid? LastModifiedBy { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
    internal const int MaxSkuLength = 50;
    internal const int MaxNameLength = 100;
    internal const int MaxDescriptionLength = 1000;

    public static Product Create(Guid organisationId, string sku, string name, string? description, Guid unitOfMeasureId, Guid? productCategoryId, Guid taxCategoryId, DateTimeOffset createdAt, Guid createdBy)
    {
        ValidateRequiredIdentifiers(organisationId, unitOfMeasureId, productCategoryId, taxCategoryId, createdBy);
        var product = new Product()
        {
            Id = Guid.NewGuid(),
            OrganisationId = organisationId,
            Sku = NormalizeSku(sku),
            Name = NormalizeName(name),
            Description = NormalizeDescription(description),
            UnitOfMeasureId = unitOfMeasureId,
            ProductCategoryId = productCategoryId,
            TaxCategoryId = taxCategoryId,
            CreatedAt = createdAt.ToUniversalTime(),
            CreatedBy = createdBy,
            Status = ProductStatus.Active,
            LastModifiedAt = null,
            LastModifiedBy = null,
            RowVersion = Array.Empty<byte>()

        };

        return product;

    }
    private static string NormalizeSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new BusinessRuleException(ProductErrors.SkuRequiredCode, ProductErrors.SkuRequiredMessage);
        }

        string normalizedSku = sku.Trim().ToUpperInvariant();

        if (normalizedSku.Length > MaxSkuLength)
        {
            throw new BusinessRuleException(ProductErrors.SkuInvalidCode, ProductErrors.SkuOverLengthMessage);
        }

        if (normalizedSku.Any(char.IsWhiteSpace))
        {
            throw new BusinessRuleException(ProductErrors.SkuInvalidCode, ProductErrors.SkuInvalidMessage);
        }

        if (!normalizedSku.All(x => (x >= 'A' && x <= 'Z') || (x >= '0' && x <= '9') || x == '-'))
        {
            throw new BusinessRuleException(ProductErrors.SkuInvalidCode, ProductErrors.SkuInvalidCharactersMessage);
        }

        return normalizedSku;
    }
    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException(ProductErrors.NameRequiredCode, ProductErrors.NameRequiredMessage);
        }
        string normalizedName = Regex.Replace(name.Trim(), @"\s+", " ");

        if (normalizedName.Length > MaxNameLength)
        {
            throw new BusinessRuleException(ProductErrors.NameInvalidCode, ProductErrors.NameOverLengthMessage);
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
            throw new BusinessRuleException(ProductErrors.DescriptionInvalidCode, ProductErrors.DescriptionInvalidMessage);

        }
        return normalizedDescription;

    }
    private static void ValidateRequiredIdentifiers(Guid organisationId, Guid unitOfMeasureId, Guid? productCategoryId, Guid taxCategoryId, Guid createdBy)
    {
        EnsureNotEmpty(organisationId, ProductErrors.OrganisationRequiredCode, ProductErrors.OrganisationRequiredMessage);
        EnsureNotEmpty(unitOfMeasureId, ProductErrors.UnitOfMeasureRequiredCode, ProductErrors.UnitOfMeasureRequiredMessage);
        if (productCategoryId.HasValue && productCategoryId.Value == Guid.Empty)
        {
            throw new BusinessRuleException(ProductErrors.ProductCategoryInvalidCode, ProductErrors.ProductCategoryInvalidMessage);
        }
        EnsureNotEmpty(taxCategoryId, ProductErrors.TaxCategoryRequiredCode, ProductErrors.TaxCategoryRequiredMessage);
        EnsureNotEmpty(createdBy, ProductErrors.CreatedByRequiredCode, ProductErrors.CreatedByRequiredMessage);

    }
    private void ValidateModificationAudit(DateTimeOffset modifiedAt, Guid modifiedBy)
    {
        EnsureNotEmpty(modifiedBy, ProductErrors.ModifiedByRequiredCode, ProductErrors.ModifiedByRequiredMessage);
        if (modifiedAt == default || modifiedAt < CreatedAt)
        {
            throw new BusinessRuleException(ProductErrors.ModifiedAtInvalidCode, ProductErrors.ModifiedAtInvalidMessage);
        }

    }
    private static void EnsureNotEmpty(Guid value, string errorCode, string errorMessage)
    {
        if (value == Guid.Empty)
        {
            throw new BusinessRuleException(errorCode, errorMessage);
        }

    }
    public void Deactivate(DateTimeOffset modifiedAt, Guid modifiedBy)
    {
        ValidateModificationAudit(modifiedAt, modifiedBy);
        if (Status == ProductStatus.Inactive)
        {
            return;
        }
        Status = ProductStatus.Inactive;
        LastModifiedAt = modifiedAt.ToUniversalTime();
        LastModifiedBy = modifiedBy;
    }
    public void Activate(DateTimeOffset modifiedAt, Guid modifiedBy)
    {
        ValidateModificationAudit(modifiedAt, modifiedBy);
        if (Status == ProductStatus.Active)
        {
            return;
        }
        Status = ProductStatus.Active;
        LastModifiedAt = modifiedAt.ToUniversalTime();
        LastModifiedBy = modifiedBy;

    }

    public void UpdateDetails(string name, string? description, Guid? productCategoryId, Guid taxCategoryId, DateTimeOffset modifiedAt, Guid modifiedBy)
    {
        ValidateModificationAudit(modifiedAt, modifiedBy);

        EnsureNotEmpty(taxCategoryId, ProductErrors.TaxCategoryRequiredCode, ProductErrors.TaxCategoryRequiredMessage);

        if (productCategoryId.HasValue && productCategoryId.Value == Guid.Empty)
        {
            throw new BusinessRuleException(ProductErrors.ProductCategoryInvalidCode, ProductErrors.ProductCategoryInvalidMessage);
        }
        string normalizedName = NormalizeName(name);
        string normalizedDescription = NormalizeDescription(description);

        bool hasNoChanges =
            Name == normalizedName &&
            Description == normalizedDescription &&
            ProductCategoryId == productCategoryId &&
            TaxCategoryId == taxCategoryId;
        if (hasNoChanges)
        {
            return;
        }

        Name = normalizedName;
        Description = normalizedDescription;
        ProductCategoryId = productCategoryId;
        TaxCategoryId = taxCategoryId;
        LastModifiedAt = modifiedAt.ToUniversalTime();
        LastModifiedBy = modifiedBy;

    }
    private Product()
    {


    }
}
