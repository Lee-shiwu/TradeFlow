namespace TradeFlow.Modules.Purchasing.Application.Suppliers.ListSuppliers;

public sealed record ListSuppliersResult(
    IReadOnlyList<ListSupplierItem> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
