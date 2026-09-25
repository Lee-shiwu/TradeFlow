using TradeFlow.Modules.Purchasing.Domain.Suppliers;

namespace TradeFlow.Api.Endpoints.Purchasing.Suppliers;

public sealed record ListSuppliersRequest
{
    public string? Search { get; init; }
    public SupplierStatus? Status { get; init; }
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
}
