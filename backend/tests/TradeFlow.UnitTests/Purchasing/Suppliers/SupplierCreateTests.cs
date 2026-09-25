using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Purchasing.Domain.Suppliers;

namespace TradeFlow.UnitTests.Purchasing.Suppliers;

public sealed class SupplierCreateTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 9, 25, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidValues_NormalizesAndActivatesSupplier()
    {
        Guid organisationId = Guid.NewGuid();
        Guid createdBy = Guid.NewGuid();

        Supplier supplier =
            Supplier.Create(
                organisationId,
                " supplier-01 ",
                "  Auckland   Office Supplies  ",
                CreatedAt,
                createdBy);

        Assert.NotEqual(Guid.Empty, supplier.Id);
        Assert.Equal(organisationId, supplier.OrganisationId);
        Assert.Equal("SUPPLIER-01", supplier.Code);
        Assert.Equal("Auckland Office Supplies", supplier.Name);
        Assert.Equal(SupplierStatus.Active, supplier.Status);
        Assert.Equal(CreatedAt, supplier.CreatedAt);
        Assert.Equal(createdBy, supplier.CreatedBy);
        Assert.Empty(supplier.RowVersion);
    }

    [Theory]
    [InlineData("", "SUPPLIER_CODE_REQUIRED")]
    [InlineData("invalid code", "SUPPLIER_CODE_INVALID")]
    [InlineData("INVALID_", "SUPPLIER_CODE_INVALID")]
    public void Create_WithInvalidCode_ThrowsBusinessRule(
        string code,
        string expectedCode)
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                Supplier.Create(
                    Guid.NewGuid(),
                    code,
                    "Supplier",
                    CreatedAt,
                    Guid.NewGuid()));

        Assert.Equal(expectedCode, exception.Code);
    }

    [Fact]
    public void Create_WithBlankName_ThrowsBusinessRule()
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                Supplier.Create(
                    Guid.NewGuid(),
                    "SUPPLIER",
                    " ",
                    CreatedAt,
                    Guid.NewGuid()));

        Assert.Equal("SUPPLIER_NAME_REQUIRED", exception.Code);
    }

    [Theory]
    [InlineData(true, false, "SUPPLIER_ORGANISATION_REQUIRED")]
    [InlineData(false, true, "SUPPLIER_CREATED_BY_REQUIRED")]
    public void Create_WithMissingIdentity_ThrowsBusinessRule(
        bool emptyOrganisation,
        bool emptyCreatedBy,
        string expectedCode)
    {
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(() =>
                Supplier.Create(
                    emptyOrganisation ? Guid.Empty : Guid.NewGuid(),
                    "SUPPLIER",
                    "Supplier",
                    CreatedAt,
                    emptyCreatedBy ? Guid.Empty : Guid.NewGuid()));

        Assert.Equal(expectedCode, exception.Code);
    }
}
