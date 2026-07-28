using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.UnitsOfMeasure;

namespace TradeFlow.UnitTests.Catalog.UnitsOfMeasure;

public sealed class UnitOfMeasureTests
{
    [Theory]
    [InlineData("EA", "Each")]
    [InlineData("KG", "Kilogram")]
    [InlineData("L", "Litre")]
    [InlineData("M", "Metre")]
    public void Create_WithSupportedCode_CreatesExpectedUnit(
        string code,
        string expectedName)
    {
        // Act
        UnitOfMeasure unitOfMeasure =
            UnitOfMeasure.Create(code);

        // Assert
        Assert.NotEqual(Guid.Empty, unitOfMeasure.Id);
        Assert.Equal(code, unitOfMeasure.Code);
        Assert.Equal(expectedName, unitOfMeasure.Name);
        Assert.Equal(
            UnitOfMeasureStatus.Active,
            unitOfMeasure.Status);

        Assert.Empty(unitOfMeasure.RowVersion);
    }

    [Fact]
    public void Create_WithLowercaseAndOuterWhitespace_NormalizesCode()
    {
        // Act
        UnitOfMeasure unitOfMeasure =
            UnitOfMeasure.Create(" kg ");

        // Assert
        Assert.Equal("KG", unitOfMeasure.Code);
        Assert.Equal("Kilogram", unitOfMeasure.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_WithMissingCode_ThrowsBusinessRuleException(
        string? code)
    {
        // Act
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => UnitOfMeasure.Create(code));

        // Assert
        Assert.Equal(
            "UNIT_OF_MEASURE_CODE_REQUIRED",
            exception.Code);

        Assert.Equal(
            "Unit of measure code is required.",
            exception.Message);
    }

    [Fact]
    public void Create_WithCodeOverMaxLength_ThrowsBusinessRuleException()
    {
        // Arrange
        string code = new('A', 11);

        // Act
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => UnitOfMeasure.Create(code));

        // Assert
        Assert.Equal(
            "UNIT_OF_MEASURE_CODE_INVALID",
            exception.Code);

        Assert.Equal(
            "Unit of measure code must not exceed 10 characters.",
            exception.Message);
    }

    [Theory]
    [InlineData("K-G")]
    [InlineData("KG1")]
    [InlineData("KG_")]
    [InlineData("公斤")]
    public void Create_WithInvalidCharacters_ThrowsBusinessRuleException(
        string code)
    {
        // Act
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => UnitOfMeasure.Create(code));

        // Assert
        Assert.Equal(
            "UNIT_OF_MEASURE_CODE_INVALID",
            exception.Code);

        Assert.Equal(
            "Unit of measure code can only contain letters from A to Z.",
            exception.Message);
    }

    [Theory]
    [InlineData("BOX")]
    [InlineData("PACK")]
    [InlineData("ABC")]
    public void Create_WithUnsupportedCode_ThrowsBusinessRuleException(
        string code)
    {
        // Act
        BusinessRuleException exception =
            Assert.Throws<BusinessRuleException>(
                () => UnitOfMeasure.Create(code));

        // Assert
        Assert.Equal(
            "UNIT_OF_MEASURE_CODE_UNSUPPORTED",
            exception.Code);

        Assert.Equal(
            "Unit of measure code is not supported.",
            exception.Message);
    }

    [Fact]
    public void Create_GeneratesDifferentIdForEachUnit()
    {
        // Act
        UnitOfMeasure first =
            UnitOfMeasure.Create("EA");

        UnitOfMeasure second =
            UnitOfMeasure.Create("EA");

        // Assert
        Assert.NotEqual(first.Id, second.Id);
    }
}
