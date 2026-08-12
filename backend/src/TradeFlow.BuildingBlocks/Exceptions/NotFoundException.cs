namespace TradeFlow.BuildingBlocks.Exceptions;

public sealed class NotFoundException : Exception
{

    public NotFoundException(string code, string message) : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;

    }
    public string Code { get; }

}
