using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using Xunit;

namespace TradeFlow.UnitTests.Catalog.TaxCategories;

public sealed class TaxCategoryEffectivePeriodTests
{
    [Fact]
    public void Create_WithoutEffectiveTo_CreatesOpenEndedCategory()
    {
        DateOnly effectiveFrom = new(2026, 4, 1);

        TaxCategory category =
            CreateCategory(effectiveFrom, null);

        Assert.Equal(
            effectiveFrom,
            category.EffectiveFrom);

        Assert.Null(category.EffectiveTo);
    }

    [Fact]
    public void Create_WithEffectiveToAfterEffectiveFrom_CreatesCategory()
    {
        DateOnly effectiveFrom = new(2026, 4, 1);
        DateOnly effectiveTo = new(2027, 3, 31);

        TaxCategory category =
            CreateCategory(
                effectiveFrom,
                effectiveTo);

        Assert.Equal(
            effectiveFrom,
            category.EffectiveFrom);

        Assert.Equal(
            effectiveTo,
            category.EffectiveTo);
    }

    [Fact]
    public void Create_WithSameEffectiveDates_CreatesOneDayCategory()
    {
        DateOnly effectiveDate = new(2026, 4, 1);

        TaxCategory category =
            CreateCategory(
                effectiveDate,
                effectiveDate);

        Assert.Equal(
            effectiveDate,
            category.EffectiveFrom);

        Assert.Equal(
            effectiveDate,
            category.EffectiveTo);
    }

    [Fact]
    public void Create_WithDefaultEffectiveFrom_ThrowsBusinessRuleException()
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                CreateCategory(default, null));

        Assert.Equal(
            "TAX_CATEGORY_EFFECTIVE_FROM_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Tax category effective-from date is required.",
            exception.Message);
    }

    [Fact]
    public void Create_WithEffectiveToBeforeEffectiveFrom_ThrowsBusinessRuleException()
    {
        DateOnly effectiveFrom = new(2026, 4, 1);
        DateOnly effectiveTo = new(2026, 3, 31);

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                CreateCategory(
                    effectiveFrom,
                    effectiveTo));

        Assert.Equal(
            "TAX_CATEGORY_EFFECTIVE_PERIOD_INVALID",
            exception.Code);

        Assert.Equal(
            "Tax category effective-to date cannot be earlier than the effective-from date.",
            exception.Message);
    }

    private static TaxCategory CreateCategory(
        DateOnly effectiveFrom,
        DateOnly? effectiveTo)
    {
        return TaxCategory.Create(
            code: "GST15",
            name: "Standard GST",
            description: string.Empty,
            rate: 0.1500m,
            treatment: TaxTreatment.StandardRated,
            effectiveFrom: effectiveFrom,
            effectiveTo: effectiveTo);
    }
}
