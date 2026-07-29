using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using Xunit;

namespace TradeFlow.UnitTests.Catalog.ProductCategories;

public sealed class ProductCategoryStatusTests
{
    [Fact]
    public void Deactivate_ActiveCategory_ChangesStatusToInactive()
    {
        ProductCategory category = CreateCategory();
        Guid modifiedBy = Guid.NewGuid();
        DateTimeOffset modifiedAt = new(
            2026,
            7,
            30,
            10,
            30,
            0,
            TimeSpan.FromHours(12));

        category.Deactivate(modifiedAt, modifiedBy);

        Assert.Equal(
            ProductCategoryStatus.Inactive,
            category.Status);

        Assert.Equal(
            modifiedAt.ToUniversalTime(),
            category.LastModifiedAt);

        Assert.Equal(modifiedBy, category.LastModifiedBy);
    }

    [Fact]
    public void Deactivate_ConvertsModifiedAtToUtc()
    {
        ProductCategory category = CreateCategory();

        DateTimeOffset modifiedAt = new(
            2026,
            7,
            30,
            10,
            30,
            0,
            TimeSpan.FromHours(12));

        category.Deactivate(
            modifiedAt,
            Guid.NewGuid());

        Assert.NotNull(category.LastModifiedAt);
        Assert.Equal(
            TimeSpan.Zero,
            category.LastModifiedAt.Value.Offset);

        Assert.Equal(
            modifiedAt.ToUniversalTime(),
            category.LastModifiedAt);
    }

    [Fact]
    public void Deactivate_AlreadyInactive_DoesNotChangeAuditInformation()
    {
        ProductCategory category = CreateCategory();

        Guid firstModifiedBy = Guid.NewGuid();

        DateTimeOffset firstModifiedAt = new(
            2026,
            7,
            30,
            1,
            0,
            0,
            TimeSpan.Zero);

        category.Deactivate(
            firstModifiedAt,
            firstModifiedBy);

        Guid secondModifiedBy = Guid.NewGuid();

        DateTimeOffset secondModifiedAt =
            firstModifiedAt.AddHours(2);

        category.Deactivate(
            secondModifiedAt,
            secondModifiedBy);

        Assert.Equal(
            ProductCategoryStatus.Inactive,
            category.Status);

        Assert.Equal(
            firstModifiedAt,
            category.LastModifiedAt);

        Assert.Equal(
            firstModifiedBy,
            category.LastModifiedBy);
    }

    [Fact]
    public void Activate_InactiveCategory_ChangesStatusToActive()
    {
        ProductCategory category = CreateCategory();

        category.Deactivate(
            new DateTimeOffset(
                2026,
                7,
                30,
                1,
                0,
                0,
                TimeSpan.Zero),
            Guid.NewGuid());

        Guid activatedBy = Guid.NewGuid();

        DateTimeOffset activatedAt = new(
            2026,
            7,
            30,
            15,
            30,
            0,
            TimeSpan.FromHours(12));

        category.Activate(
            activatedAt,
            activatedBy);

        Assert.Equal(
            ProductCategoryStatus.Active,
            category.Status);

        Assert.Equal(
            activatedAt.ToUniversalTime(),
            category.LastModifiedAt);

        Assert.Equal(
            activatedBy,
            category.LastModifiedBy);
    }

    [Fact]
    public void Activate_AlreadyActive_DoesNotSetAuditInformation()
    {
        ProductCategory category = CreateCategory();

        category.Activate(
            DateTimeOffset.UtcNow,
            Guid.NewGuid());

        Assert.Equal(
            ProductCategoryStatus.Active,
            category.Status);

        Assert.Null(category.LastModifiedAt);
        Assert.Null(category.LastModifiedBy);
    }

    [Fact]
    public void Deactivate_WithEmptyModifiedBy_ThrowsBusinessRuleException()
    {
        ProductCategory category = CreateCategory();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                category.Deactivate(
                    DateTimeOffset.UtcNow,
                    Guid.Empty));

        Assert.Equal(
            "PRODUCT_CATEGORY_MODIFIED_BY_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product category modifier is required.",
            exception.Message);
    }

    [Fact]
    public void Deactivate_WithDefaultModifiedAt_ThrowsBusinessRuleException()
    {
        ProductCategory category = CreateCategory();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                category.Deactivate(
                    default,
                    Guid.NewGuid()));

        Assert.Equal(
            "PRODUCT_CATEGORY_MODIFIED_AT_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product category modification time is required.",
            exception.Message);
    }

    [Fact]
    public void Activate_WithEmptyModifiedBy_ThrowsBusinessRuleException()
    {
        ProductCategory category = CreateInactiveCategory();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                category.Activate(
                    DateTimeOffset.UtcNow,
                    Guid.Empty));

        Assert.Equal(
            "PRODUCT_CATEGORY_MODIFIED_BY_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product category modifier is required.",
            exception.Message);
    }

    [Fact]
    public void Activate_WithDefaultModifiedAt_ThrowsBusinessRuleException()
    {
        ProductCategory category = CreateInactiveCategory();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                category.Activate(
                    default,
                    Guid.NewGuid()));

        Assert.Equal(
            "PRODUCT_CATEGORY_MODIFIED_AT_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product category modification time is required.",
            exception.Message);
    }

    [Fact]
    public void Activate_AlreadyActiveWithInvalidAudit_StillThrowsException()
    {
        ProductCategory category = CreateCategory();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                category.Activate(
                    DateTimeOffset.UtcNow,
                    Guid.Empty));

        Assert.Equal(
            "PRODUCT_CATEGORY_MODIFIED_BY_REQUIRED",
            exception.Code);

        Assert.Equal(
            ProductCategoryStatus.Active,
            category.Status);
    }

    [Fact]
    public void Deactivate_AlreadyInactiveWithInvalidAudit_StillThrowsException()
    {
        ProductCategory category = CreateInactiveCategory();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                category.Deactivate(
                    DateTimeOffset.UtcNow,
                    Guid.Empty));

        Assert.Equal(
            "PRODUCT_CATEGORY_MODIFIED_BY_REQUIRED",
            exception.Code);

        Assert.Equal(
            ProductCategoryStatus.Inactive,
            category.Status);
    }

    private static ProductCategory CreateInactiveCategory()
    {
        ProductCategory category = CreateCategory();

        category.Deactivate(
            new DateTimeOffset(
                2026,
                7,
                30,
                0,
                0,
                0,
                TimeSpan.Zero),
            Guid.NewGuid());

        return category;
    }

    private static ProductCategory CreateCategory()
    {
        return ProductCategory.Create(
            organisationId: Guid.NewGuid(),
            code: "OFFICE",
            name: "Office Products",
            description: string.Empty,
            createdAt: new DateTimeOffset(
                2026,
                7,
                29,
                0,
                0,
                0,
                TimeSpan.Zero),
            createdBy: Guid.NewGuid());
    }
}
