using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using Xunit;

namespace TradeFlow.UnitTests.Catalog.ProductCategories;

public sealed class ProductCategoryDescriptionTests
{
    [Fact]
    public void Create_WithValidDescription_AssignsDescription()
    {
        ProductCategory category =
            CreateCategory("Products used in the office.");

        Assert.Equal(
            "Products used in the office.",
            category.Description);
    }

    [Fact]
    public void Create_WithSurroundingWhitespace_TrimsDescription()
    {
        ProductCategory category =
            CreateCategory("  Products used in the office.  ");

        Assert.Equal(
            "Products used in the office.",
            category.Description);
    }

    [Fact]
    public void Create_WithInternalWhitespace_PreservesInternalWhitespace()
    {
        ProductCategory category =
            CreateCategory("Office   furniture");

        Assert.Equal(
            "Office   furniture",
            category.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDescription_SavesEmptyString(
        string? description)
    {
        ProductCategory category =
            CreateCategory(description);

        Assert.NotNull(category.Description);
        Assert.Equal(string.Empty, category.Description);
    }

    [Fact]
    public void Create_WithDescriptionAtMaximumLength_CreatesCategory()
    {
        string description = new('A', 1000);

        ProductCategory category =
            CreateCategory(description);

        Assert.Equal(description, category.Description);
        Assert.Equal(1000, category.Description.Length);
    }

    [Fact]
    public void Create_WithDescriptionOverMaximumLength_ThrowsBusinessRuleException()
    {
        string description = new('A', 1001);

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateCategory(description));

        Assert.Equal(
            "PRODUCT_CATEGORY_DESCRIPTION_INVALID",
            exception.Code);

        Assert.Equal(
            "Product category description must not exceed 1000 characters.",
            exception.Message);
    }

    private static ProductCategory CreateCategory(
        string? description)
    {
        return ProductCategory.Create(
            organisationId: Guid.NewGuid(),
            code: "OFFICE",
            name: "Office Products",
            description: description,
            createdAt: DateTimeOffset.UtcNow,
            createdBy: Guid.NewGuid());
    }
}
