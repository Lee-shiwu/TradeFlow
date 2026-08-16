namespace TradeFlow.BuildingBlocks.Exceptions;

public sealed class RequestValidationException : Exception
{
    public RequestValidationException(string code, string message) : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }
    public string Code { get; }
}
