namespace TradeFlow.Api.Endpoints.Purchasing.Suppliers;

public sealed record CreateSupplierRequest(
    string Code,
    string Name);
