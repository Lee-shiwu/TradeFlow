using System;
using System.Collections.Generic;
using System.Text;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.Products;

namespace TradeFlow.UnitTests.Catalog.Products;

public class ProductIdentifierTests
{
    [Fact]
    public void Create_WithEmptyOrganisationId_ThrowsBusinessRuleException()
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateProduct(
                    organisationId: Guid.Empty));

        Assert.Equal(
            "PRODUCT_ORGANISATION_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product organisation is required.",
            exception.Message);
    }

    [Fact]
    public void Create_WithEmptyUnitOfMeasureId_ThrowsBusinessRuleException()
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateProduct(
                    unitOfMeasureId: Guid.Empty));

        Assert.Equal(
            "PRODUCT_UNIT_OF_MEASURE_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product unit of measure is required.",
            exception.Message);
    }

    [Fact]
    public void Create_WithEmptyTaxCategoryId_ThrowsBusinessRuleException()
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateProduct(
                    taxCategoryId: Guid.Empty));

        Assert.Equal(
            "PRODUCT_TAX_CATEGORY_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product tax category is required.",
            exception.Message);
    }

    [Fact]
    public void Create_WithEmptyCreatedBy_ThrowsBusinessRuleException()
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateProduct(
                    createdBy: Guid.Empty));

        Assert.Equal(
            "PRODUCT_CREATED_BY_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product creator is required.",
            exception.Message);
    }

    [Fact]
    public void Create_WithoutProductCategory_CreatesProduct()
    {
        Product product = CreateProduct(
            productCategoryId: null);

        Assert.Null(product.ProductCategoryId);
    }

    [Fact]
    public void Create_WithValidProductCategory_AssignsCategory()
    {
        Guid productCategoryId = Guid.NewGuid();

        Product product = CreateProduct(
            productCategoryId: productCategoryId);

        Assert.Equal(
            productCategoryId,
            product.ProductCategoryId);
    }

    [Fact]
    public void Create_WithEmptyProductCategory_ThrowsBusinessRuleException()
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => CreateProduct(
                    productCategoryId: Guid.Empty));

        Assert.Equal(
            "PRODUCT_CATEGORY_INVALID",
            exception.Code);

        Assert.Equal(
            "Product category identifier is invalid.",
            exception.Message);
    }

    [Fact]
    public void Create_WithValidIdentifiers_GeneratesProductId()
    {
        Product product = CreateProduct();

        Assert.NotEqual(Guid.Empty, product.Id);
    }

    [Fact]
    public void Create_WithValidIdentifiers_AssignsIdentifiers()
    {
        Guid organisationId = Guid.NewGuid();
        Guid unitOfMeasureId = Guid.NewGuid();
        Guid taxCategoryId = Guid.NewGuid();
        Guid createdBy = Guid.NewGuid();

        Product product = CreateProduct(
            organisationId: organisationId,
            unitOfMeasureId: unitOfMeasureId,
            taxCategoryId: taxCategoryId,
            createdBy: createdBy);

        Assert.Equal(
            organisationId,
            product.OrganisationId);

        Assert.Equal(
            unitOfMeasureId,
            product.UnitOfMeasureId);

        Assert.Equal(
            taxCategoryId,
            product.TaxCategoryId);

        Assert.Equal(
            createdBy,
            product.CreatedBy);
    }

    private static Product CreateProduct(
       Guid? organisationId = null,
       Guid? unitOfMeasureId = null,
       Guid? productCategoryId = null,
       Guid? taxCategoryId = null,
       Guid? createdBy = null)
    {
        return Product.Create(
            organisationId:
                organisationId ?? Guid.NewGuid(),
            sku:
                "CHAIR-001",
            name:
                "Office Chair",
            description:
                null,
            unitOfMeasureId:
                unitOfMeasureId ?? Guid.NewGuid(),
            productCategoryId:
                productCategoryId,
            taxCategoryId:
                taxCategoryId ?? Guid.NewGuid(),
            createdAt:
                new DateTimeOffset(
                    2026,
                    7,
                    25,
                    0,
                    0,
                    0,
                    TimeSpan.Zero),
            createdBy:
                createdBy ?? Guid.NewGuid());
    }
}
