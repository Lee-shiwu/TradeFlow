namespace TradeFlow.Api.Endpoints.Purchasing.Suppliers;

public sealed record ListSuppliersResponse(
    IReadOnlyList<ListSupplierItemResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
