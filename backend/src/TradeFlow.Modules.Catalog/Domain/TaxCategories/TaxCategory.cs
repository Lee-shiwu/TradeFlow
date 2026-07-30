
using System.Text.RegularExpressions;
using TradeFlow.BuildingBlocks.Exceptions;

namespace TradeFlow.Modules.Catalog.Domain.TaxCategories;

public sealed class TaxCategory
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Rate { get; private set; }
    public TaxTreatment Treatment { get; private set; }
    public TaxCategoryStatus Status { get; private set; } = TaxCategoryStatus.Active;
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    internal const int MaxNameLength = 100;
    internal const int MaxCodeLength = 30;
    internal const int MaxDescriptionLength = 500;

    public static TaxCategory Create(string code, string name, string? description, decimal rate, TaxTreatment treatment, DateOnly effectiveFrom, DateOnly? effectiveTo)
    {
        string normalizedCode = NormalizeCode(code);
        string normalizedName = NormalizeName(name);
        string normalizedDescription = NormalizeDescription(description);
        ValidateRate(rate);
        ValidateTreatmentAndRate(treatment, rate);
        ValidateEffectivePeriod(effectiveFrom, effectiveTo);

        return new TaxCategory
        {
            Id = Guid.NewGuid(),
            Code = normalizedCode,
            Name = normalizedName,
            Description = normalizedDescription,
            Rate = rate,
            Treatment = treatment,
            Status = TaxCategoryStatus.Active,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            RowVersion = Array.Empty<byte>()

        };

    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new BusinessRuleException(TaxCategoryErrors.CodeRequiredCode, TaxCategoryErrors.CodeRequiredMessage);
        }
        string normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Length > MaxCodeLength)
        {
            throw new BusinessRuleException(TaxCategoryErrors.CodeInvalidCode, TaxCategoryErrors.CodeOverLengthMessage);
        }
        if (normalizedCode.Any(char.IsWhiteSpace))
        {
            throw new BusinessRuleException(TaxCategoryErrors.CodeInvalidCode, TaxCategoryErrors.CodeInvalidMessage);
        }
        if (!normalizedCode.All(x =>
                (x >= 'A' && x <= 'Z') ||
                (x >= '0' && x <= '9') ||
                 x == '-'))
        {
            throw new BusinessRuleException(TaxCategoryErrors.CodeInvalidCode, TaxCategoryErrors.CodeInvalidCharactersMessage);
        }
        return normalizedCode;
    }

    private static string NormalizeName(string name)
    {

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException(TaxCategoryErrors.NameRequiredCode, TaxCategoryErrors.NameRequiredMessage);
        }
        string normalizedName = Regex.Replace(name.Trim(), @"\s+", " ");

        if (normalizedName.Length > MaxNameLength)
        {
            throw new BusinessRuleException(TaxCategoryErrors.NameInvalidCode, TaxCategoryErrors.NameOverLengthMessage);
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
            throw new BusinessRuleException(TaxCategoryErrors.DescriptionInvalidCode, TaxCategoryErrors.DescriptionOverLengthMessage);
        }
        return normalizedDescription;
    }

    private static void ValidateRate(decimal rate)
    {
        if (rate < 0m || rate > 1m)
        {
            throw new BusinessRuleException(TaxCategoryErrors.RateInvalidCode, TaxCategoryErrors.RateRangeMessage);
        }
        if (decimal.Round(rate, 4) != rate)
        {
            throw new BusinessRuleException(TaxCategoryErrors.RateInvalidCode, TaxCategoryErrors.RateScaleMessage);
        }
    }

    private static void ValidateTreatmentAndRate(TaxTreatment treatment, decimal rate)
    {
        if (!Enum.IsDefined(treatment))
        {
            throw new BusinessRuleException(TaxCategoryErrors.TreatmentInvalidCode, TaxCategoryErrors.TreatmentInvalidMessage);
        }

        bool treatmentMatchesRate = treatment switch
        {
            TaxTreatment.StandardRated => rate > 0m,

            TaxTreatment.ZeroRated => rate == 0m,

            TaxTreatment.Exempt => rate == 0m,

            _ => false

        };
        if (!treatmentMatchesRate)
        {
            throw new BusinessRuleException(TaxCategoryErrors.TreatmentRateMismatchCode, TaxCategoryErrors.TreatmentRateMismatchMessage);
        }
    }

    private static void ValidateEffectivePeriod(DateOnly effectiveFrom, DateOnly? effectiveTo)
    {
        if (effectiveFrom == default)
        {
            throw new BusinessRuleException(TaxCategoryErrors.EffectiveFromRequiredCode, TaxCategoryErrors.EffectiveFromRequiredMessage);

        }
        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
        {
            throw new BusinessRuleException(TaxCategoryErrors.EffectivePeriodInvalidCode, TaxCategoryErrors.EffectivePeriodInvalidMessage);
        }

    }


    private TaxCategory()
    {

    }

}
