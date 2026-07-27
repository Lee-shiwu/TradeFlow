using System;
using System.Collections.Generic;
using System.Text;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain;
using TradeFlow.Modules.Catalog.Domain.Products;


namespace TradeFlow.UnitTests.Catalog.Products;

public sealed class ProductDescriptionTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("        ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_WithEmptyDescription_SavesEmptyString(string? description)
    {
        Product product = CreateProduct(description);

        Assert.Equal(string.Empty,product.Description);
    }

    [Fact]
    public void Create_WithDescriptionAtMaxLength_CreatesProduct()
    {
        string description = new ('A', 1000);
        Product product = CreateProduct(description);

        Assert.Equal(description, product.Description);
        Assert.Equal(1000, product.Description.Length);

    }
    [Fact]
    public void Create_WithDescriptionOverMaxLength_ThrowsBusinessRuleException()
    {
        string description = new string('A', 1001);

        BusinessRuleException exception = Assert.Throws<BusinessRuleException>(() => CreateProduct(description));

        Assert.Equal("PRODUCT_DESCRIPTION_INVALID", exception.Code);
        Assert.Equal("Product description must not exceed 1000 characters.", exception.Message);

    }
    [Fact]
    public void Create_WithDescriptionWhitespace_TrimsOuterWhitespace()
    {
       const string description = "  Adjustable office chair  ";
        Product product = CreateProduct(description);

        Assert.Equal("Adjustable office chair", product.Description);

    }
    [Fact]
    public void Create_WithMultilineDescription_PreservesLineBreaks()
    {
        // Arrange
        const string description = "Line 1\nLine 2\nLine 3";

        // Act
        Product product = CreateProduct(description);

        // Assert
        Assert.Equal(description, product.Description);
    }
    [Fact]
    public void Create_WithInternalSpaces_PreservesInternalSpaces()
    {
        // Arrange
        const string description = "Chair with    adjustable height";

        // Act
        Product product = CreateProduct(description);

        // Assert
        Assert.Equal(description, product.Description);
    }
    [Fact]
    public void Create_WithHtmlLikeDescription_PreservesItAsPlainText()
    {
        // Arrange
        const string description =
            "<script>alert('test')</script>";

        // Act
        Product product = CreateProduct(description);

        // Assert
        Assert.Equal(description, product.Description);
    }
    private static Product CreateProduct(string? description)
    {
        return Product.Create
            (
                organisationId: Guid.NewGuid(),
            sku: "CHAIR-001",
            name: "Office Chair",
            description: description,
            unitOfMeasureId: Guid.NewGuid(),
            productCategoryId: null,
            taxCategoryId: Guid.NewGuid(),
            createdAt: new DateTimeOffset(
                2026,
                7,
                24,
                0,
                0,
                0,
                TimeSpan.Zero),
            createdBy: Guid.NewGuid());



            

    }
}
