using TradeFlow.BuildingBlocks.Exceptions;

namespace TradeFlow.UnitTests;

public sealed class BusinessRuleExceptionTests
{
    [Fact]
    public void Constructor_PreservesStableErrorCode()
    {
        BusinessRuleException exception = new(
            "CAT_PRODUCT_SKU_DUPLICATE",
            "A product with this SKU already exists.");

        Assert.Equal("CAT_PRODUCT_SKU_DUPLICATE", exception.Code);
        Assert.Equal("A product with this SKU already exists.", exception.Message);
    }
}
