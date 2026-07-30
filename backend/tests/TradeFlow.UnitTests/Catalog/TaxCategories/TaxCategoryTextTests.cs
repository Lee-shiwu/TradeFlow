using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;
using Xunit;

namespace TradeFlow.UnitTests.Catalog.TaxCategories;

public sealed class TaxCategoryTextTests
{
    [Fact]
    public void Create_WithSurroundingWhitespace_TrimsName()
    {
        TaxCategory category =
            CreateCategory(name: "  Standard GST  ");

        Assert.Equal("Standard GST", category.Name);
    }

    [Fact]
    public void Create_WithRepeatedWhitespace_NormalizesName()
    {
        TaxCategory category =
            CreateCategory(name: "Standard    GST");

        Assert.Equal("Standard GST", category.Name);
    }

    [Fact]
    public void Create_WithDifferentWhitespace_NormalizesName()
    {
        TaxCategory category =
            CreateCategory(name: " Standard\t\n GST ");

        Assert.Equal("Standard GST", category.Name);
    }

    [Fact]
    public void Create_WithUnicodeName_CreatesCategory()
    {
        TaxCategory category =
            CreateCategory(name: "标准消费税");

        Assert.Equal("标准消费税", category.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ThrowsBusinessRuleException(
        string? name)
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                CreateCategory(name: name));

        Assert.Equal(
            "TAX_CATEGORY_NAME_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Tax category name is required.",
            exception.Message);
    }

    [Fact]
    public void Create_WithNameAtMaximumLength_CreatesCategory()
    {
        string name = new('A', 100);

        TaxCategory category =
            CreateCategory(name: name);

        Assert.Equal(name, category.Name);
    }

    [Fact]
    public void Create_WithNameOverMaximumLength_ThrowsBusinessRuleException()
    {
        string name = new('A', 101);

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                CreateCategory(name: name));

        Assert.Equal(
            "TAX_CATEGORY_NAME_INVALID",
            exception.Code);

        Assert.Equal(
            "Tax category name must not exceed 100 characters.",
            exception.Message);
    }

    [Fact]
    public void Create_WithSurroundingWhitespace_TrimsDescription()
    {
        TaxCategory category = CreateCategory(
            description: "  Standard GST rate.  ");

        Assert.Equal(
            "Standard GST rate.",
            category.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDescription_SavesEmptyString(
        string? description)
    {
        TaxCategory category =
            CreateCategory(description: description);

        Assert.NotNull(category.Description);
        Assert.Equal(
            string.Empty,
            category.Description);
    }

    [Fact]
    public void Create_WithInternalWhitespace_PreservesDescription()
    {
        TaxCategory category = CreateCategory(
            description: "Standard   GST rate.");

        Assert.Equal(
            "Standard   GST rate.",
            category.Description);
    }

    [Fact]
    public void Create_WithDescriptionAtMaximumLength_CreatesCategory()
    {
        string description = new('A', 500);

        TaxCategory category =
            CreateCategory(description: description);

        Assert.Equal(description, category.Description);
    }

    [Fact]
    public void Create_WithDescriptionOverMaximumLength_ThrowsBusinessRuleException()
    {
        string description = new('A', 501);

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                CreateCategory(description: description));

        Assert.Equal(
            "TAX_CATEGORY_DESCRIPTION_INVALID",
            exception.Code);

        Assert.Equal(
            "Tax category description must not exceed 500 characters.",
            exception.Message);
    }

    private static TaxCategory CreateCategory(
        string? name = "Standard GST",
        string? description = "")
    {
        return TaxCategory.Create(
            code: "GST15",
            name: name!,
            description: description,
            rate: 0.1500m,
            treatment: TaxTreatment.StandardRated,
            effectiveFrom: new DateOnly(2026, 4, 1),
            effectiveTo: null);
    }
}
