using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.Products;

namespace TradeFlow.UnitTests.Catalog.Products;

public sealed class ProductUpdateDetailsTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(
            2026,
            7,
            25,
            0,
            0,
            0,
            TimeSpan.Zero);

    private static readonly DateTimeOffset ModifiedAt =
        CreatedAt.AddHours(1);

    private static readonly Guid OriginalCategoryId =
        Guid.NewGuid();

    private static readonly Guid OriginalTaxCategoryId =
        Guid.NewGuid();

    [Fact]
    public void UpdateDetails_WithNewName_NormalizesAndUpdatesName()
    {
        Product product = CreateProduct();
        Guid modifiedBy = Guid.NewGuid();

        product.UpdateDetails(
            name: " Adjustable   Office Chair ",
            description: product.Description,
            productCategoryId: product.ProductCategoryId,
            taxCategoryId: product.TaxCategoryId,
            modifiedAt: ModifiedAt,
            modifiedBy: modifiedBy);

        Assert.Equal(
            "Adjustable Office Chair",
            product.Name);

        Assert.Equal(
            (DateTimeOffset?)ModifiedAt,
            product.LastModifiedAt);

        Assert.Equal(
            (Guid?)modifiedBy,
            product.LastModifiedBy);
    }

    [Fact]
    public void UpdateDetails_WithNewDescription_UpdatesDescription()
    {
        Product product = CreateProduct();

        product.UpdateDetails(
            name: product.Name,
            description: "  New product description  ",
            productCategoryId: product.ProductCategoryId,
            taxCategoryId: product.TaxCategoryId,
            modifiedAt: ModifiedAt,
            modifiedBy: Guid.NewGuid());

        Assert.Equal(
            "New product description",
            product.Description);
    }

    [Fact]
    public void UpdateDetails_WithNewProductCategory_UpdatesCategory()
    {
        Product product = CreateProduct();
        Guid newCategoryId = Guid.NewGuid();

        product.UpdateDetails(
            name: product.Name,
            description: product.Description,
            productCategoryId: newCategoryId,
            taxCategoryId: product.TaxCategoryId,
            modifiedAt: ModifiedAt,
            modifiedBy: Guid.NewGuid());

        Assert.Equal(
            (Guid?)newCategoryId,
            product.ProductCategoryId);
    }

    [Fact]
    public void UpdateDetails_WithNullProductCategory_ClearsCategory()
    {
        Product product = CreateProduct();

        product.UpdateDetails(
            name: product.Name,
            description: product.Description,
            productCategoryId: null,
            taxCategoryId: product.TaxCategoryId,
            modifiedAt: ModifiedAt,
            modifiedBy: Guid.NewGuid());

        Assert.Null(product.ProductCategoryId);
    }

    [Fact]
    public void UpdateDetails_WithEmptyProductCategory_ThrowsBusinessRuleException()
    {
        Product product = CreateProduct();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => product.UpdateDetails(
                    name: product.Name,
                    description: product.Description,
                    productCategoryId: Guid.Empty,
                    taxCategoryId: product.TaxCategoryId,
                    modifiedAt: ModifiedAt,
                    modifiedBy: Guid.NewGuid()));

        Assert.Equal(
            "PRODUCT_CATEGORY_INVALID",
            exception.Code);

        Assert.Equal(
            "Product category identifier is invalid.",
            exception.Message);
    }

    [Fact]
    public void UpdateDetails_WithNewTaxCategory_UpdatesTaxCategory()
    {
        Product product = CreateProduct();
        Guid newTaxCategoryId = Guid.NewGuid();

        product.UpdateDetails(
            name: product.Name,
            description: product.Description,
            productCategoryId: product.ProductCategoryId,
            taxCategoryId: newTaxCategoryId,
            modifiedAt: ModifiedAt,
            modifiedBy: Guid.NewGuid());

        Assert.Equal(
            newTaxCategoryId,
            product.TaxCategoryId);
    }

    [Fact]
    public void UpdateDetails_WithEmptyTaxCategory_ThrowsBusinessRuleException()
    {
        Product product = CreateProduct();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => product.UpdateDetails(
                    name: product.Name,
                    description: product.Description,
                    productCategoryId: product.ProductCategoryId,
                    taxCategoryId: Guid.Empty,
                    modifiedAt: ModifiedAt,
                    modifiedBy: Guid.NewGuid()));

        Assert.Equal(
            "PRODUCT_TAX_CATEGORY_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product tax category is required.",
            exception.Message);
    }

    [Fact]
    public void UpdateDetails_WithNoActualChanges_DoesNotUpdateAuditInformation()
    {
        Product product = CreateProduct();

        product.UpdateDetails(
            name: "  Office   Chair  ",
            description: "  Original description  ",
            productCategoryId: product.ProductCategoryId,
            taxCategoryId: product.TaxCategoryId,
            modifiedAt: ModifiedAt,
            modifiedBy: Guid.NewGuid());

        Assert.Equal("Office Chair", product.Name);
        Assert.Equal(
            "Original description",
            product.Description);

        Assert.Null(product.LastModifiedAt);
        Assert.Null(product.LastModifiedBy);
    }

    [Fact]
    public void UpdateDetails_DoesNotChangeCreationAuditInformation()
    {
        Guid createdBy = Guid.NewGuid();
        Product product = CreateProduct(createdBy);

        DateTimeOffset originalCreatedAt =
            product.CreatedAt;

        Guid originalCreatedBy =
            product.CreatedBy;

        product.UpdateDetails(
            name: "Updated Chair",
            description: product.Description,
            productCategoryId: product.ProductCategoryId,
            taxCategoryId: product.TaxCategoryId,
            modifiedAt: ModifiedAt,
            modifiedBy: Guid.NewGuid());

        Assert.Equal(
            originalCreatedAt,
            product.CreatedAt);

        Assert.Equal(
            originalCreatedBy,
            product.CreatedBy);
    }

    [Fact]
    public void UpdateDetails_WithEmptyModifiedBy_ThrowsBusinessRuleException()
    {
        Product product = CreateProduct();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => product.UpdateDetails(
                    name: "Updated Chair",
                    description: product.Description,
                    productCategoryId: product.ProductCategoryId,
                    taxCategoryId: product.TaxCategoryId,
                    modifiedAt: ModifiedAt,
                    modifiedBy: Guid.Empty));

        Assert.Equal(
            "PRODUCT_MODIFIED_BY_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product modifier is required.",
            exception.Message);
    }

    [Fact]
    public void UpdateDetails_WithTimeBeforeCreation_ThrowsBusinessRuleException()
    {
        Product product = CreateProduct();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => product.UpdateDetails(
                    name: "Updated Chair",
                    description: product.Description,
                    productCategoryId: product.ProductCategoryId,
                    taxCategoryId: product.TaxCategoryId,
                    modifiedAt: CreatedAt.AddMinutes(-1),
                    modifiedBy: Guid.NewGuid()));

        Assert.Equal(
            "PRODUCT_MODIFIED_AT_INVALID",
            exception.Code);

        Assert.Equal(
            "Product modification time is invalid.",
            exception.Message);
    }

    [Fact]
    public void UpdateDetails_InactiveProduct_UpdatesDetailsAndRemainsInactive()
    {
        Product product = CreateProduct();

        product.Deactivate(
            CreatedAt.AddHours(1),
            Guid.NewGuid());

        product.UpdateDetails(
            name: "Updated Inactive Chair",
            description: product.Description,
            productCategoryId: product.ProductCategoryId,
            taxCategoryId: product.TaxCategoryId,
            modifiedAt: CreatedAt.AddHours(2),
            modifiedBy: Guid.NewGuid());

        Assert.Equal(
            "Updated Inactive Chair",
            product.Name);

        Assert.Equal(
            ProductStatus.Inactive,
            product.Status);
    }

    private static Product CreateProduct(
        Guid? createdBy = null)
    {
        return Product.Create(
            organisationId: Guid.NewGuid(),
            sku: "CHAIR-001",
            name: "Office Chair",
            description: "Original description",
            unitOfMeasureId: Guid.NewGuid(),
            productCategoryId: OriginalCategoryId,
            taxCategoryId: OriginalTaxCategoryId,
            createdAt: CreatedAt,
            createdBy: createdBy ?? Guid.NewGuid());
    }
}
