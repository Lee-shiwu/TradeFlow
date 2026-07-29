using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using Xunit;

namespace TradeFlow.UnitTests.Catalog.ProductCategories;

public sealed class ProductCategoryUpdateDetailsTests
{
    [Fact]
    public void UpdateDetails_WithNewName_UpdatesName()
    {
        ProductCategory category = CreateCategory();
        Guid modifiedBy = Guid.NewGuid();
        DateTimeOffset modifiedAt = CreateModifiedAt();

        category.UpdateDetails(
            name: "Workplace Products",
            description: category.Description,
            modifiedAt: modifiedAt,
            modifiedBy: modifiedBy);

        Assert.Equal("Workplace Products", category.Name);
        Assert.Equal(
            modifiedAt.ToUniversalTime(),
            category.LastModifiedAt);
        Assert.Equal(modifiedBy, category.LastModifiedBy);
    }

    [Fact]
    public void UpdateDetails_WithNewDescription_UpdatesDescription()
    {
        ProductCategory category = CreateCategory();
        Guid modifiedBy = Guid.NewGuid();
        DateTimeOffset modifiedAt = CreateModifiedAt();

        category.UpdateDetails(
            name: category.Name,
            description: "Products used in workplaces.",
            modifiedAt: modifiedAt,
            modifiedBy: modifiedBy);

        Assert.Equal(
            "Products used in workplaces.",
            category.Description);

        Assert.Equal(
            modifiedAt.ToUniversalTime(),
            category.LastModifiedAt);

        Assert.Equal(modifiedBy, category.LastModifiedBy);
    }

    [Fact]
    public void UpdateDetails_WithNewNameAndDescription_UpdatesBothValues()
    {
        ProductCategory category = CreateCategory();

        Guid originalId = category.Id;
        Guid originalOrganisationId = category.OrganisationId;
        string originalCode = category.Code;
        DateTimeOffset originalCreatedAt = category.CreatedAt;
        Guid originalCreatedBy = category.CreatedBy;

        category.UpdateDetails(
            name: "Workplace Equipment",
            description: "Equipment used in workplaces.",
            modifiedAt: CreateModifiedAt(),
            modifiedBy: Guid.NewGuid());

        Assert.Equal("Workplace Equipment", category.Name);
        Assert.Equal(
            "Equipment used in workplaces.",
            category.Description);

        // 不允许 UpdateDetails 修改稳定字段。
        Assert.Equal(originalId, category.Id);
        Assert.Equal(
            originalOrganisationId,
            category.OrganisationId);
        Assert.Equal(originalCode, category.Code);
        Assert.Equal(originalCreatedAt, category.CreatedAt);
        Assert.Equal(originalCreatedBy, category.CreatedBy);
    }

    [Fact]
    public void UpdateDetails_NormalizesNameAndDescription()
    {
        ProductCategory category = CreateCategory();

        category.UpdateDetails(
            name: "  Workplace    Equipment  ",
            description: "  Equipment used in workplaces.  ",
            modifiedAt: CreateModifiedAt(),
            modifiedBy: Guid.NewGuid());

        Assert.Equal(
            "Workplace Equipment",
            category.Name);

        Assert.Equal(
            "Equipment used in workplaces.",
            category.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_WithEmptyDescription_SavesEmptyString(
        string? description)
    {
        ProductCategory category = CreateCategory();

        category.UpdateDetails(
            name: category.Name,
            description: description,
            modifiedAt: CreateModifiedAt(),
            modifiedBy: Guid.NewGuid());

        Assert.NotNull(category.Description);
        Assert.Equal(string.Empty, category.Description);
    }

    [Fact]
    public void UpdateDetails_WithNoActualChanges_DoesNotUpdateAuditInformation()
    {
        ProductCategory category = CreateCategory();

        category.UpdateDetails(
            name: "  Office    Products  ",
            description: "  Products used in the office.  ",
            modifiedAt: CreateModifiedAt(),
            modifiedBy: Guid.NewGuid());

        Assert.Equal("Office Products", category.Name);
        Assert.Equal(
            "Products used in the office.",
            category.Description);

        Assert.Null(category.LastModifiedAt);
        Assert.Null(category.LastModifiedBy);
    }

    [Fact]
    public void UpdateDetails_ConvertsModifiedAtToUtc()
    {
        ProductCategory category = CreateCategory();

        DateTimeOffset modifiedAt = new(
            2026,
            7,
            30,
            15,
            30,
            0,
            TimeSpan.FromHours(12));

        category.UpdateDetails(
            name: "Workplace Products",
            description: category.Description,
            modifiedAt: modifiedAt,
            modifiedBy: Guid.NewGuid());

        Assert.NotNull(category.LastModifiedAt);

        Assert.Equal(
            TimeSpan.Zero,
            category.LastModifiedAt.Value.Offset);

        Assert.Equal(
            modifiedAt.ToUniversalTime(),
            category.LastModifiedAt);
    }

    [Fact]
    public void UpdateDetails_InactiveCategory_UpdatesDetails()
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

        Guid modifiedBy = Guid.NewGuid();
        DateTimeOffset modifiedAt = CreateModifiedAt();

        category.UpdateDetails(
            name: "Archived Office Products",
            description: "Historical office products.",
            modifiedAt: modifiedAt,
            modifiedBy: modifiedBy);

        Assert.Equal(
            ProductCategoryStatus.Inactive,
            category.Status);

        Assert.Equal(
            "Archived Office Products",
            category.Name);

        Assert.Equal(
            "Historical office products.",
            category.Description);

        Assert.Equal(
            modifiedAt.ToUniversalTime(),
            category.LastModifiedAt);

        Assert.Equal(modifiedBy, category.LastModifiedBy);
    }

    [Fact]
    public void UpdateDetails_WithEmptyModifiedBy_ThrowsBusinessRuleException()
    {
        ProductCategory category = CreateCategory();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                category.UpdateDetails(
                    name: "Workplace Products",
                    description: category.Description,
                    modifiedAt: CreateModifiedAt(),
                    modifiedBy: Guid.Empty));

        Assert.Equal(
            "PRODUCT_CATEGORY_MODIFIED_BY_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product category modifier is required.",
            exception.Message);
    }

    [Fact]
    public void UpdateDetails_WithDefaultModifiedAt_ThrowsBusinessRuleException()
    {
        ProductCategory category = CreateCategory();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                category.UpdateDetails(
                    name: "Workplace Products",
                    description: category.Description,
                    modifiedAt: default,
                    modifiedBy: Guid.NewGuid()));

        Assert.Equal(
            "PRODUCT_CATEGORY_MODIFIED_AT_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product category modification time is required.",
            exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_WithEmptyName_ThrowsBusinessRuleException(
        string? name)
    {
        ProductCategory category = CreateCategory();

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                category.UpdateDetails(
                    name: name!,
                    description: category.Description,
                    modifiedAt: CreateModifiedAt(),
                    modifiedBy: Guid.NewGuid()));

        Assert.Equal(
            "PRODUCT_CATEGORY_NAME_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product category name is required.",
            exception.Message);
    }

    [Fact]
    public void UpdateDetails_WithNameOverMaximumLength_ThrowsBusinessRuleException()
    {
        ProductCategory category = CreateCategory();
        string name = new('A', 101);

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                category.UpdateDetails(
                    name: name,
                    description: category.Description,
                    modifiedAt: CreateModifiedAt(),
                    modifiedBy: Guid.NewGuid()));

        Assert.Equal(
            "PRODUCT_CATEGORY_NAME_INVALID",
            exception.Code);

        Assert.Equal(
            "Product category name must not exceed 100 characters.",
            exception.Message);
    }

    [Fact]
    public void UpdateDetails_WithDescriptionOverMaximumLength_ThrowsBusinessRuleException()
    {
        ProductCategory category = CreateCategory();
        string description = new('A', 1001);

        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                category.UpdateDetails(
                    name: category.Name,
                    description: description,
                    modifiedAt: CreateModifiedAt(),
                    modifiedBy: Guid.NewGuid()));

        Assert.Equal(
            "PRODUCT_CATEGORY_DESCRIPTION_INVALID",
            exception.Code);

        Assert.Equal(
            "Product category description must not exceed 1000 characters.",
            exception.Message);
    }

    [Fact]
    public void UpdateDetails_WithInvalidName_DoesNotPartiallyModifyCategory()
    {
        ProductCategory category = CreateCategory();

        string originalName = category.Name;
        string originalDescription = category.Description;

        Assert.Throws<BusinessRuleException>(() =>
            category.UpdateDetails(
                name: string.Empty,
                description: "A new description.",
                modifiedAt: CreateModifiedAt(),
                modifiedBy: Guid.NewGuid()));

        Assert.Equal(originalName, category.Name);
        Assert.Equal(
            originalDescription,
            category.Description);
        Assert.Null(category.LastModifiedAt);
        Assert.Null(category.LastModifiedBy);
    }

    [Fact]
    public void UpdateDetails_WithInvalidDescription_DoesNotPartiallyModifyCategory()
    {
        ProductCategory category = CreateCategory();
        string description = new('A', 1001);

        string originalName = category.Name;
        string originalDescription = category.Description;

        Assert.Throws<BusinessRuleException>(() =>
            category.UpdateDetails(
                name: "Workplace Products",
                description: description,
                modifiedAt: CreateModifiedAt(),
                modifiedBy: Guid.NewGuid()));

        Assert.Equal(originalName, category.Name);
        Assert.Equal(
            originalDescription,
            category.Description);
        Assert.Null(category.LastModifiedAt);
        Assert.Null(category.LastModifiedBy);
    }

    private static DateTimeOffset CreateModifiedAt()
    {
        return new DateTimeOffset(
            2026,
            7,
            30,
            15,
            30,
            0,
            TimeSpan.FromHours(12));
    }

    private static ProductCategory CreateCategory()
    {
        return ProductCategory.Create(
            organisationId: Guid.NewGuid(),
            code: "OFFICE",
            name: "Office Products",
            description: "Products used in the office.",
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
