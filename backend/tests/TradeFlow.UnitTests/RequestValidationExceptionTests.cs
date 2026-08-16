using TradeFlow.BuildingBlocks.Exceptions;

namespace TradeFlow.UnitTests;

public sealed class RequestValidationExceptionTests
{
    [Fact]
    public void Constructor_PreservesCodeAndMessage()
    {
        RequestValidationException exception =
            new(
                code: "LIST_PRODUCTS_PAGE_NUMBER_INVALID",
                message:
                    "Page number must be greater than or equal to 1.");

        Assert.Equal(
            "LIST_PRODUCTS_PAGE_NUMBER_INVALID",
            exception.Code);

        Assert.Equal(
            "Page number must be greater than or equal to 1.",
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
            () => new RequestValidationException(
                code!,
                "The request is invalid."));
    }
}
