namespace TradeFlow.Api.Endpoints.Inventory;

public sealed class ListStockRequest
{
    public string? Search { get; init; }
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
}
