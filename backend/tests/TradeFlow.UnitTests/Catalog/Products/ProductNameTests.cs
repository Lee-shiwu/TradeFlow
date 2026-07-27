using System;
using System.Collections.Generic;
using System.Text;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.Products;

namespace TradeFlow.UnitTests.Catalog.Products;



public sealed class ProductNameTests
{
    [Theory]
    [InlineData(" Office Chair ", "Office Chair")]
    [InlineData("Office   Chair", "Office Chair")]
    [InlineData("Office\tChair", "Office Chair")]
    [InlineData("Office\nChair", "Office Chair")]
    public void Create_WithValidName_NormalizesName(
        string name,
        string expectedName)
    {
        // Act
        Product product = CreateProduct(name);

        // Assert
        Assert.Equal(expectedName, product.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_WithEmptyName_ThrowsBusinessRuleException(
        string name)
    {
        // Act
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateProduct(name));

        // Assert
        Assert.Equal("PRODUCT_NAME_REQUIRED", exception.Code);
        Assert.Equal(
            "Product name is required.",
            exception.Message);
    }

    [Fact]
    public void Create_WithNullName_ThrowsBusinessRuleException()
    {
        // Act
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateProduct(null!));

        // Assert
        Assert.Equal("PRODUCT_NAME_REQUIRED", exception.Code);
        Assert.Equal(
            "Product name is required.",
            exception.Message);
    }

    [Fact]
    public void Create_WithNameAtMaxLength_CreatesProduct()
    {
        // Arrange
        string name = new('A', 100);

        // Act
        Product product = CreateProduct(name);

        // Assert
        Assert.Equal(name, product.Name);
        Assert.Equal(100, product.Name.Length);
    }

    [Fact]
    public void Create_WithNameOverMaxLength_ThrowsBusinessRuleException()
    {
        // Arrange
        string name = new('A', 101);

        // Act
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateProduct(name));

        // Assert
        Assert.Equal("PRODUCT_NAME_INVALID", exception.Code);
        Assert.Equal(
            "Product name must not exceed 100 characters.",
            exception.Message);
    }

    [Theory]
    [InlineData("Tēpu Māori")]
    [InlineData("办公椅")]
    public void Create_WithUnicodeName_PreservesName(string name)
    {
        // Act
        Product product = CreateProduct(name);

        // Assert
        Assert.Equal(name, product.Name);
    }

    private static Product CreateProduct(string name)
    {
        return Product.Create(
            organisationId: Guid.NewGuid(),
            sku: "CHAIR-001",
            name: name,
            description: string.Empty,
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

