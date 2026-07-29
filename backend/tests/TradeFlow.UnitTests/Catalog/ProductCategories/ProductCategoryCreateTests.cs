using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;
using Xunit;

namespace TradeFlow.UnitTests.Catalog.ProductCategories;

public sealed class ProductCategoryCreateTests
{
    private static readonly Guid OrganisationId =
        Guid.Parse("10000000-0000-0000-0000-000000000001");

    private static readonly Guid CreatedBy =
        Guid.Parse("20000000-0000-0000-0000-000000000001");

    [Fact]
    public void Create_WithValidValues_CreatesActiveProductCategory()
    {
        DateTimeOffset createdAt = new(
            2026,
            7,
            29,
            10,
            30,
            0,
            TimeSpan.FromHours(12));

        ProductCategory category = ProductCategory.Create(
            organisationId: OrganisationId,
            code: "OFFICE",
            name: "Office Products",
            description: "Products used in the office.",
            createdAt: createdAt,
            createdBy: CreatedBy);

        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal(OrganisationId, category.OrganisationId);
        Assert.Equal("OFFICE", category.Code);
        Assert.Equal("Office Products", category.Name);
        Assert.Equal(
            "Products used in the office.",
            category.Description);
        Assert.Equal(
            ProductCategoryStatus.Active,
            category.Status);
        Assert.Equal(
            createdAt.ToUniversalTime(),
            category.CreatedAt);
        Assert.Equal(CreatedBy, category.CreatedBy);
        Assert.Null(category.LastModifiedAt);
        Assert.Null(category.LastModifiedBy);
        Assert.Empty(category.RowVersion);
    }

    [Fact]
    public void Create_GeneratesDifferentIdForEachCategory()
    {
        ProductCategory firstCategory = CreateCategory();
        ProductCategory secondCategory = CreateCategory();

        Assert.NotEqual(firstCategory.Id, secondCategory.Id);
    }

    [Fact]
    public void Create_ConvertsCreatedAtToUtc()
    {
        DateTimeOffset createdAt = new(
            2026,
            7,
            29,
            10,
            30,
            0,
            TimeSpan.FromHours(12));

        ProductCategory category = ProductCategory.Create(
            organisationId: OrganisationId,
            code: "OFFICE",
            name: "Office Products",
            description: string.Empty,
            createdAt: createdAt,
            createdBy: CreatedBy);

        Assert.Equal(
            TimeSpan.Zero,
            category.CreatedAt.Offset);

        Assert.Equal(
            createdAt.ToUniversalTime(),
            category.CreatedAt);
    }

    [Fact]
    public void Create_WithEmptyOrganisationId_ThrowsBusinessRuleException()
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                ProductCategory.Create(
                    organisationId: Guid.Empty,
                    code: "OFFICE",
                    name: "Office Products",
                    description: string.Empty,
                    createdAt: DateTimeOffset.UtcNow,
                    createdBy: CreatedBy));

        Assert.Equal(
            "PRODUCT_CATEGORY_ORGANISATION_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product category organisation is required.",
            exception.Message);
    }

    [Fact]
    public void Create_WithEmptyCreatedBy_ThrowsBusinessRuleException()
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                ProductCategory.Create(
                    organisationId: OrganisationId,
                    code: "OFFICE",
                    name: "Office Products",
                    description: string.Empty,
                    createdAt: DateTimeOffset.UtcNow,
                    createdBy: Guid.Empty));

        Assert.Equal(
            "PRODUCT_CATEGORY_CREATED_BY_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product category creator is required.",
            exception.Message);
    }

    [Fact]
    public void Create_WithDefaultCreatedAt_ThrowsBusinessRuleException()
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                ProductCategory.Create(
                    organisationId: OrganisationId,
                    code: "OFFICE",
                    name: "Office Products",
                    description: string.Empty,
                    createdAt: default,
                    createdBy: CreatedBy));

        Assert.Equal(
            "PRODUCT_CATEGORY_CREATED_AT_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Product category creation time is required.",
            exception.Message);
    }

    private static ProductCategory CreateCategory()
    {
        return ProductCategory.Create(
            organisationId: OrganisationId,
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
            createdBy: CreatedBy);
    }
}
