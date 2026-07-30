using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using Xunit;

namespace TradeFlow.UnitTests.Catalog.TaxCategories;

public sealed class TaxCategoryCodeTests
{
    [Fact]
    public void Create_WithValidCode_NormalizesCode()
    {
        TaxCategory category =
            CreateCategory(" gst-15 ");

        Assert.Equal("GST-15", category.Code);
    }

    [Theory]
    [InlineData("GST15")]
    [InlineData("GST-15")]
    [InlineData("ZERO")]
    [InlineData("123")]
    [InlineData("A-1")]
    public void Create_WithAllowedCodeCharacters_CreatesCategory(
        string code)
    {
        TaxCategory category = CreateCategory(code);

        Assert.Equal(code, category.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyCode_ThrowsBusinessRuleException(
        string? code)
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateCategory(code));

        Assert.Equal(
            "TAX_CATEGORY_CODE_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Tax category code is required.",
            exception.Message);
    }

    [Theory]
    [InlineData("GST 15")]
    [InlineData("GST\t15")]
    [InlineData("GST\n15")]
    public void Create_WithWhitespaceInsideCode_ThrowsBusinessRuleException(
        string code)
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateCategory(code));

        Assert.Equal(
            "TAX_CATEGORY_CODE_INVALID",
            exception.Code);

        Assert.Equal(
            "Tax category code must not contain whitespace.",
            exception.Message);
    }

    [Theory]
    [InlineData("GST_15")]
    [InlineData("GST.15")]
    [InlineData("GST@15")]
    [InlineData("GST/15")]
    [InlineData("税务")]
    [InlineData("ＧＳＴ１５")]
    public void Create_WithInvalidCodeCharacters_ThrowsBusinessRuleException(
        string code)
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateCategory(code));

        Assert.Equal(
            "TAX_CATEGORY_CODE_INVALID",
            exception.Code);

        Assert.Equal(
            "Tax category code can only consist of numbers, uppercase letters, and hyphens.",
            exception.Message);
    }

    [Fact]
    public void Create_WithCodeAtMaximumLength_CreatesCategory()
    {
        string code = new('A', 30);

        TaxCategory category = CreateCategory(code);

        Assert.Equal(code, category.Code);
        Assert.Equal(30, category.Code.Length);
    }

    [Fact]
    public void Create_WithCodeOverMaximumLength_ThrowsBusinessRuleException()
    {
        string code = new('A', 31);

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateCategory(code));

        Assert.Equal(
            "TAX_CATEGORY_CODE_INVALID",
            exception.Code);

        Assert.Equal(
            "Tax category code must not exceed 30 characters.",
            exception.Message);
    }

    private static TaxCategory CreateCategory(string? code)
    {
        return TaxCategory.Create(
            code: code!,
            name: "Standard GST",
            description: string.Empty,
            rate: 0.1500m,
            treatment: TaxTreatment.StandardRated,
            effectiveFrom: new DateOnly(2026, 4, 1),
            effectiveTo: null);
    }
}
