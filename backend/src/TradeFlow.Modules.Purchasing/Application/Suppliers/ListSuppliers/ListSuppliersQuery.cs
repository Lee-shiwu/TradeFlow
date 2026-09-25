using TradeFlow.Modules.Purchasing.Domain.Suppliers;

namespace TradeFlow.Modules.Purchasing.Application.Suppliers.ListSuppliers;

public sealed record ListSuppliersQuery(
    Guid OrganisationId,
    string? Search,
    SupplierStatus? Status,
    int PageNumber,
    int PageSize);
