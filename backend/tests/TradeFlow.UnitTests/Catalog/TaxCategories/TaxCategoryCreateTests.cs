using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using Xunit;

namespace TradeFlow.UnitTests.Catalog.TaxCategories;

public sealed class TaxCategoryCreateTests
{
    [Fact]
    public void Create_WithValidValues_CreatesActiveTaxCategory()
    {
        DateOnly effectiveFrom = new(2026, 4, 1);
        DateOnly effectiveTo = new(2027, 3, 31);

        TaxCategory category = TaxCategory.Create(
            code: "GST15",
            name: "Standard GST",
            description: "New Zealand standard GST rate.",
            rate: 0.1500m,
            treatment: TaxTreatment.StandardRated,
            effectiveFrom: effectiveFrom,
            effectiveTo: effectiveTo);

        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal("GST15", category.Code);
        Assert.Equal("Standard GST", category.Name);
        Assert.Equal(
            "New Zealand standard GST rate.",
            category.Description);
        Assert.Equal(0.1500m, category.Rate);
        Assert.Equal(
            TaxTreatment.StandardRated,
            category.Treatment);
        Assert.Equal(
            TaxCategoryStatus.Active,
            category.Status);
        Assert.Equal(effectiveFrom, category.EffectiveFrom);
        Assert.Equal(effectiveTo, category.EffectiveTo);
        Assert.Empty(category.RowVersion);
    }

    [Fact]
    public void Create_GeneratesDifferentIdForEachCategory()
    {
        TaxCategory firstCategory = CreateCategory();
        TaxCategory secondCategory = CreateCategory();

        Assert.NotEqual(
            firstCategory.Id,
            secondCategory.Id);
    }

    [Fact]
    public void Create_WithoutEffectiveTo_CreatesOpenEndedCategory()
    {
        TaxCategory category = TaxCategory.Create(
            code: "GST15",
            name: "Standard GST",
            description: string.Empty,
            rate: 0.1500m,
            treatment: TaxTreatment.StandardRated,
            effectiveFrom: new DateOnly(2026, 4, 1),
            effectiveTo: null);

        Assert.Null(category.EffectiveTo);
    }

    private static TaxCategory CreateCategory()
    {
        return TaxCategory.Create(
            code: "GST15",
            name: "Standard GST",
            description: string.Empty,
            rate: 0.1500m,
            treatment: TaxTreatment.StandardRated,
            effectiveFrom: new DateOnly(2026, 4, 1),
            effectiveTo: null);
    }
}
