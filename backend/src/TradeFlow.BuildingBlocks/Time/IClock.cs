namespace TradeFlow.BuildingBlocks.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
