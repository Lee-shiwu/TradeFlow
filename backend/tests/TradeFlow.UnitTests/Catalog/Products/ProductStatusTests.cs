using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.Products;

namespace TradeFlow.UnitTests.Catalog.Products;

public sealed class ProductStatusTests
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

    [Fact]
    public void Create_NewProduct_DefaultsToActive()
    {
        Product product = CreateProduct();

        Assert.Equal(ProductStatus.Active, product.Status);
        Assert.Null(product.LastModifiedAt);
        Assert.Null(product.LastModifiedBy);
    }

    [Fact]
    public void Deactivate_ActiveProduct_ChangesStatusAndAuditInformation()
    {
        Product product = CreateProduct();
        DateTimeOffset modifiedAt = CreatedAt.AddHours(1);
        Guid modifiedBy = Guid.NewGuid();

        product.Deactivate(modifiedAt, modifiedBy);

        Assert.Equal(ProductStatus.Inactive, product.Status);
        Assert.Equal(
            (DateTimeOffset?)modifiedAt,
            product.LastModifiedAt);
        Assert.Equal(
            (Guid?)modifiedBy,
            product.LastModifiedBy);
    }

    [Fact]
    public void Deactivate_DoesNotChangeCreationAuditInformation()
    {
        Guid createdBy = Guid.NewGuid();
        Product product = CreateProduct(createdBy);
        DateTimeOffset originalCreatedAt = product.CreatedAt;
        Guid originalCreatedBy = product.CreatedBy;

        product.Deactivate(
            CreatedAt.AddHours(1),
            Guid.NewGuid());

        Assert.Equal(originalCreatedAt, product.CreatedAt);
        Assert.Equal(originalCreatedBy, product.CreatedBy);
    }

    [Fact]
    public void Deactivate_InactiveProduct_DoesNotChangeExistingAuditInformation()
    {
        Product product = CreateProduct();
        DateTimeOffset firstModifiedAt = CreatedAt.AddHours(1);
        Guid firstModifiedBy = Guid.NewGuid();

        product.Deactivate(
            firstModifiedAt,
            firstModifiedBy);

        product.Deactivate(
            CreatedAt.AddHours(2),
            Guid.NewGuid());

        Assert.Equal(ProductStatus.Inactive, product.Status);
        Assert.Equal(
            (DateTimeOffset?)firstModifiedAt,
            product.LastModifiedAt);
        Assert.Equal(
            (Guid?)firstModifiedBy,
            product.LastModifiedBy);
    }

    [Fact]
    public void Activate_InactiveProduct_ChangesStatusAndAuditInformation()
    {
        Product product = CreateProduct();

        product.Deactivate(
            CreatedAt.AddHours(1),
            Guid.NewGuid());

        DateTimeOffset activatedAt = CreatedAt.AddHours(2);
        Guid activatedBy = Guid.NewGuid();

        product.Activate(
            activatedAt,
            activatedBy);

        Assert.Equal(ProductStatus.Active, product.Status);
        Assert.Equal(
            (DateTimeOffset?)activatedAt,
            product.LastModifiedAt);
        Assert.Equal(
            (Guid?)activatedBy,
            product.LastModifiedBy);
    }

    [Fact]
    public void Activate_ActiveProduct_DoesNotChangeExistingAuditInformation()
    {
        Product product = CreateProduct();

        product.Deactivate(
            CreatedAt.AddHours(1),
            Guid.NewGuid());

        DateTimeOffset firstActivatedAt =
            CreatedAt.AddHours(2);

        Guid firstActivatedBy = Guid.NewGuid();

        product.Activate(
            firstActivatedAt,
            firstActivatedBy);

        product.Activate(
            CreatedAt.AddHours(3),
            Guid.NewGuid());

        Assert.Equal(ProductStatus.Active, product.Status);
        Assert.Equal(
            (DateTimeOffset?)firstActivatedAt,
            product.LastModifiedAt);
        Assert.Equal(
            (Guid?)firstActivatedBy,
            product.LastModifiedBy);
    }

    [Fact]
    public void Deactivate_WithEmptyModifiedBy_ThrowsBusinessRuleException()
    {
        Product product = CreateProduct();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => product.Deactivate(
                    CreatedAt.AddHours(1),
                    Guid.Empty));

        Assert.Equal(
            "PRODUCT_MODIFIED_BY_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product modifier is required.",
            exception.Message);
    }

    [Fact]
    public void Activate_WithEmptyModifiedBy_ThrowsBusinessRuleException()
    {
        Product product = CreateProduct();

        product.Deactivate(
            CreatedAt.AddHours(1),
            Guid.NewGuid());

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => product.Activate(
                    CreatedAt.AddHours(2),
                    Guid.Empty));

        Assert.Equal(
            "PRODUCT_MODIFIED_BY_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product modifier is required.",
            exception.Message);
    }

    [Fact]
    public void Deactivate_WithDefaultModifiedAt_ThrowsBusinessRuleException()
    {
        Product product = CreateProduct();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => product.Deactivate(
                    default,
                    Guid.NewGuid()));

        Assert.Equal(
            "PRODUCT_MODIFIED_AT_INVALID",
            exception.Code);

        Assert.Equal(
            "Product modification time is invalid.",
            exception.Message);
    }

    [Fact]
    public void Deactivate_WithTimeBeforeCreation_ThrowsBusinessRuleException()
    {
        Product product = CreateProduct();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => product.Deactivate(
                    CreatedAt.AddMinutes(-1),
                    Guid.NewGuid()));

        Assert.Equal(
            "PRODUCT_MODIFIED_AT_INVALID",
            exception.Code);
    }

    [Fact]
    public void Deactivate_WithNonUtcTime_SavesTimeAsUtc()
    {
        Product product = CreateProduct();

        DateTimeOffset modifiedAt =
            new(
                2026,
                7,
                25,
                13,
                0,
                0,
                TimeSpan.FromHours(12));

        product.Deactivate(
            modifiedAt,
            Guid.NewGuid());

        Assert.Equal(
            (DateTimeOffset?)modifiedAt.ToUniversalTime(),
            product.LastModifiedAt);

        Assert.Equal(
            TimeSpan.Zero,
            product.LastModifiedAt!.Value.Offset);
    }

    private static Product CreateProduct(
        Guid? createdBy = null)
    {
        return Product.Create(
            organisationId: Guid.NewGuid(),
            sku: "CHAIR-001",
            name: "Office Chair",
            description: null,
            unitOfMeasureId: Guid.NewGuid(),
            productCategoryId: null,
            taxCategoryId: Guid.NewGuid(),
            createdAt: CreatedAt,
            createdBy: createdBy ?? Guid.NewGuid());
    }
}
