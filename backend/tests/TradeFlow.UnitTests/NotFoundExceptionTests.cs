using TradeFlow.BuildingBlocks.Exceptions;

namespace TradeFlow.UnitTests;

public sealed class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_PreservesCodeAndMessage()
    {
        NotFoundException exception = new(
            code: "PRODUCT_NOT_FOUND",
            message: "The requested product was not found.");

        Assert.Equal(
            "PRODUCT_NOT_FOUND",
            exception.Code);

        Assert.Equal(
            "The requested product was not found.",
            exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCode_ThrowsArgumentException(
        string? code)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new NotFoundException(
                code!,
                "The requested resource was not found."));
    }
}
