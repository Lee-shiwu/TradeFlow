using System;
using System.Collections.Generic;
using System.Text;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.BuildingBlocks.Exceptions;

namespace TradeFlow.UnitTests.Catalog.Products;




public sealed class ProductSkuTests
{
    [Fact]
    public void Create_WithValidSku_NormalizesSku()
    {
        Product product = CreateProduct(" chair-001 ");

        Assert.Equal("CHAIR-001", product.Sku);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptySku_ThrowsBusinessRuleException(string sku)
    {
        BusinessRuleException exception = Assert.Throws<BusinessRuleException>(
            () => CreateProduct(sku));

        Assert.Equal("PRODUCT_SKU_REQUIRED", exception.Code);
        Assert.Equal("Product SKU is required.", exception.Message);
    }

    [Fact]
    public void Create_WithNullSku_ThrowsBusinessRuleException()
    {
        BusinessRuleException exception = Assert.Throws<BusinessRuleException>(
            () => CreateProduct(null!));

        Assert.Equal("PRODUCT_SKU_REQUIRED", exception.Code);
    }

    [Theory]
    [InlineData("CHAIR 001")]
    [InlineData("CHAIR\t001")]
    [InlineData("CHAIR\n001")]
    [InlineData("椅子 001")]
    public void Create_WithWhitespaceInsideSku_ThrowsBusinessRuleException(
        string sku)
    {
        BusinessRuleException exception = Assert.Throws<BusinessRuleException>(
            () => CreateProduct(sku));

        Assert.Equal("PRODUCT_SKU_INVALID", exception.Code);
    }
    [Theory]
    [InlineData("CHAIR")]
    [InlineData("123456")]
    [InlineData("CHAIR001")]
    [InlineData("CHAIR-001")]
    [InlineData("A-1")]
    public void Create_WithAllowedSkuCharacters_CreatesProduct(string sku)
    {
        Product product = CreateProduct(sku);

        Assert.Equal(sku, product.Sku);
    }

    [Theory]
    [InlineData("CHAIR_001")]
    [InlineData("CHAIR.001")]
    [InlineData("CHAIR@001")]
    [InlineData("CHAIR/001")]
    [InlineData("椅子-001")]
    [InlineData("ＣＨＡＩＲ-001")]
    public void Create_WithInvalidSkuCharacters_ThrowsBusinessRuleException(
        string sku)
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateProduct(sku));

        Assert.Equal("PRODUCT_SKU_INVALID", exception.Code);
        Assert.Equal("Product SKU can only consist of numbers, uppercase letters, and hyphens.", exception.Message);
    }
    [Fact]
    public void Create_WithSkuAtMaxLength_CreatesProduct()
    {
        string sku = new string('A', 50);
        Product product = CreateProduct (sku);

        Assert.Equal(sku, product.Sku);
        Assert.Equal(50, product.Sku.Length);
    }

    [Fact]
    public void Create_WithSkuOverMaxLength_ThrowsBusinessRuleException()
    {
        string sku = new string('A', 51);
        BusinessRuleException exception = Assert.Throws<BusinessRuleException>(() => CreateProduct(sku));
        Assert.Equal("PRODUCT_SKU_INVALID", exception.Code);
        Assert.Equal(
            "Product SKU must not exceed 50 characters.",
            exception.Message);

    }
    private static Product CreateProduct(string sku)
    {
        return Product.Create(
            organisationId: Guid.NewGuid(),
            sku: sku,
            name: "Office Chair",
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
