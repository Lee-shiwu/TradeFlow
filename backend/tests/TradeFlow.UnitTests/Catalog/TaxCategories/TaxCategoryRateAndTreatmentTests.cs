using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using Xunit;

namespace TradeFlow.UnitTests.Catalog.TaxCategories;

public sealed class TaxCategoryRateAndTreatmentTests
{
    public static TheoryData<decimal, TaxTreatment>
        ValidTreatmentRates =>
        new()
        {
            { 0.1500m, TaxTreatment.StandardRated },
            { 0.0001m, TaxTreatment.StandardRated },
            { 1m, TaxTreatment.StandardRated },
            { 0m, TaxTreatment.ZeroRated },
            { 0m, TaxTreatment.Exempt }
        };

    public static TheoryData<decimal, TaxTreatment>
        MismatchedTreatmentRates =>
        new()
        {
            { 0m, TaxTreatment.StandardRated },
            { 0.1500m, TaxTreatment.ZeroRated },
            { 0.1500m, TaxTreatment.Exempt }
        };

    public static TheoryData<decimal>
        OutOfRangeRates =>
        new()
        {
            -0.0001m,
            1.0001m
        };

    [Theory]
    [MemberData(nameof(ValidTreatmentRates))]
    public void Create_WithMatchingTreatmentAndRate_CreatesCategory(
        decimal rate,
        TaxTreatment treatment)
    {
        TaxCategory category =
            CreateCategory(rate, treatment);

        Assert.Equal(rate, category.Rate);
        Assert.Equal(treatment, category.Treatment);
    }

    [Theory]
    [MemberData(nameof(OutOfRangeRates))]
    public void Create_WithRateOutsideAllowedRange_ThrowsBusinessRuleException(
        decimal rate)
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                CreateCategory(
                    rate,
                    TaxTreatment.StandardRated));

        Assert.Equal(
            "TAX_CATEGORY_RATE_INVALID",
            exception.Code);

        Assert.Equal(
            "Tax category rate must be between 0 and 1.",
            exception.Message);
    }

    [Fact]
    public void Create_WithMoreThanFourDecimalPlaces_ThrowsBusinessRuleException()
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                CreateCategory(
                    0.12345m,
                    TaxTreatment.StandardRated));

        Assert.Equal(
            "TAX_CATEGORY_RATE_INVALID",
            exception.Code);

        Assert.Equal(
            "Tax category rate must not exceed 4 decimal places.",
            exception.Message);
    }

    [Theory]
    [MemberData(nameof(MismatchedTreatmentRates))]
    public void Create_WithMismatchedTreatmentAndRate_ThrowsBusinessRuleException(
        decimal rate,
        TaxTreatment treatment)
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                CreateCategory(rate, treatment));

        Assert.Equal(
            "TAX_CATEGORY_TREATMENT_RATE_MISMATCH",
            exception.Code);

        Assert.Equal(
            "Tax category rate does not match its treatment.",
            exception.Message);
    }

    [Fact]
    public void Create_WithUndefinedTreatment_ThrowsBusinessRuleException()
    {
        TaxTreatment invalidTreatment =
            (TaxTreatment)999;

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                CreateCategory(
                    0.1500m,
                    invalidTreatment));

        Assert.Equal(
            "TAX_CATEGORY_TREATMENT_INVALID",
            exception.Code);

        Assert.Equal(
            "Tax category treatment is invalid.",
            exception.Message);
    }

    private static TaxCategory CreateCategory(
        decimal rate,
        TaxTreatment treatment)
    {
        return TaxCategory.Create(
            code: "GST",
            name: "GST category",
            description: string.Empty,
            rate: rate,
            treatment: treatment,
            effectiveFrom: new DateOnly(2026, 4, 1),
            effectiveTo: null);
    }
}
