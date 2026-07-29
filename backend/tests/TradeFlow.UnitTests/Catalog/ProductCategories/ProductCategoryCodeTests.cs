using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using Xunit;

namespace TradeFlow.UnitTests.Catalog.ProductCategories;

public sealed class ProductCategoryCodeTests
{
    [Fact]
    public void Create_WithValidCode_NormalizesCode()
    {
        ProductCategory category = CreateCategory(" office-001 ");

        Assert.Equal("OFFICE-001", category.Code);
    }

    [Theory]
    [InlineData("OFFICE")]
    [InlineData("OFFICE-001")]
    [InlineData("123456")]
    [InlineData("A-1")]
    [InlineData("A")]
    public void Create_WithAllowedCodeCharacters_CreatesCategory(
        string code)
    {
        ProductCategory category = CreateCategory(code);

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
            "PRODUCT_CATEGORY_CODE_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product category code is required.",
            exception.Message);
    }

    [Theory]
    [InlineData("OFFICE PRODUCT")]
    [InlineData("OFFICE\tPRODUCT")]
    [InlineData("OFFICE\nPRODUCT")]
    public void Create_WithWhitespaceInsideCode_ThrowsBusinessRuleException(
        string code)
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateCategory(code));

        Assert.Equal(
            "PRODUCT_CATEGORY_CODE_INVALID",
            exception.Code);

        Assert.Equal(
            "Product category code must not contain whitespace.",
            exception.Message);
    }

    [Theory]
    [InlineData("OFFICE_PRODUCT")]
    [InlineData("OFFICE.PRODUCT")]
    [InlineData("OFFICE@PRODUCT")]
    [InlineData("OFFICE/PRODUCT")]
    [InlineData("办公产品")]
    [InlineData("ＣＡＴＥＧＯＲＹ")]
    public void Create_WithInvalidCodeCharacters_ThrowsBusinessRuleException(
        string code)
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateCategory(code));

        Assert.Equal(
            "PRODUCT_CATEGORY_CODE_INVALID",
            exception.Code);

        Assert.Equal(
            "Product category code can only consist of numbers, uppercase letters, and hyphens.",
            exception.Message);
    }

    [Fact]
    public void Create_WithCodeAtMaximumLength_CreatesCategory()
    {
        string code = new('A', 50);

        ProductCategory category = CreateCategory(code);

        Assert.Equal(code, category.Code);
        Assert.Equal(50, category.Code.Length);
    }

    [Fact]
    public void Create_WithCodeOverMaximumLength_ThrowsBusinessRuleException()
    {
        string code = new('A', 51);

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateCategory(code));

        Assert.Equal(
            "PRODUCT_CATEGORY_CODE_INVALID",
            exception.Code);

        Assert.Equal(
            "Product category code must not exceed 50 characters.",
            exception.Message);
    }

    private static ProductCategory CreateCategory(string? code)
    {
        return ProductCategory.Create(
            organisationId: Guid.NewGuid(),
            code: code!,
            name: "Office Products",
            description: string.Empty,
            createdAt: DateTimeOffset.UtcNow,
            createdBy: Guid.NewGuid());
    }
}
