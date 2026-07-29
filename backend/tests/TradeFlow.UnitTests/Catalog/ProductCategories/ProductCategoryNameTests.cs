using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using Xunit;

namespace TradeFlow.UnitTests.Catalog.ProductCategories;

public sealed class ProductCategoryNameTests
{
    [Fact]
    public void Create_WithValidName_AssignsName()
    {
        ProductCategory category =
            CreateCategory("Office Products");

        Assert.Equal("Office Products", category.Name);
    }

    [Fact]
    public void Create_WithSurroundingWhitespace_TrimsName()
    {
        ProductCategory category =
            CreateCategory("  Office Products  ");

        Assert.Equal("Office Products", category.Name);
    }

    [Fact]
    public void Create_WithRepeatedWhitespace_CollapsesWhitespace()
    {
        ProductCategory category =
            CreateCategory("Office    Products");

        Assert.Equal("Office Products", category.Name);
    }

    [Fact]
    public void Create_WithDifferentWhitespaceCharacters_NormalizesName()
    {
        ProductCategory category =
            CreateCategory(" Office\t\n Products ");

        Assert.Equal("Office Products", category.Name);
    }

    [Fact]
    public void Create_WithUnicodeName_CreatesCategory()
    {
        ProductCategory category =
            CreateCategory("办公家具");

        Assert.Equal("办公家具", category.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ThrowsBusinessRuleException(
        string? name)
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateCategory(name));

        Assert.Equal(
            "PRODUCT_CATEGORY_NAME_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product category name is required.",
            exception.Message);
    }

    [Fact]
    public void Create_WithNameAtMaximumLength_CreatesCategory()
    {
        string name = new('A', 100);

        ProductCategory category = CreateCategory(name);

        Assert.Equal(name, category.Name);
        Assert.Equal(100, category.Name.Length);
    }

    [Fact]
    public void Create_WithNameOverMaximumLength_ThrowsBusinessRuleException()
    {
        string name = new('A', 101);

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateCategory(name));

        Assert.Equal(
            "PRODUCT_CATEGORY_NAME_INVALID",
            exception.Code);

        Assert.Equal(
            "Product category name must not exceed 100 characters.",
            exception.Message);
    }

    private static ProductCategory CreateCategory(string? name)
    {
        return ProductCategory.Create(
            organisationId: Guid.NewGuid(),
            code: "OFFICE",
            name: name!,
            description: string.Empty,
            createdAt: DateTimeOffset.UtcNow,
            createdBy: Guid.NewGuid());
    }
}
