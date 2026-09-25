using TradeFlow.Modules.Purchasing.Domain.Suppliers;

namespace TradeFlow.Modules.Purchasing.Application.Suppliers.ListSuppliers;

public sealed record ListSupplierItem(
    Guid SupplierId,
    string Code,
    string Name,
    SupplierStatus Status,
    DateTimeOffset CreatedAt);
