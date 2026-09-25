namespace TradeFlow.Modules.Purchasing.Application.Suppliers.CreateSupplier;

public sealed record CreateSupplierCommand(
    Guid OrganisationId,
    string Code,
    string Name,
    Guid CreatedBy);
