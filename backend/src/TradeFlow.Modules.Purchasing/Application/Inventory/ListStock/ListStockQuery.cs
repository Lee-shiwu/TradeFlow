namespace TradeFlow.Modules.Purchasing.Application.Inventory.ListStock;

public sealed record ListStockQuery(
    Guid OrganisationId,
    string? Search,
    int PageNumber,
    int PageSize);
