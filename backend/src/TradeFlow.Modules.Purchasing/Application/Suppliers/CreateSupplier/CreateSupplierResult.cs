using TradeFlow.Modules.Purchasing.Domain.Suppliers;

namespace TradeFlow.Modules.Purchasing.Application.Suppliers.CreateSupplier;

public sealed record CreateSupplierResult(
    Guid SupplierId,
    string Code,
    string Name,
    SupplierStatus Status,
    DateTimeOffset CreatedAt,
    Guid CreatedBy,
    byte[] RowVersion);
