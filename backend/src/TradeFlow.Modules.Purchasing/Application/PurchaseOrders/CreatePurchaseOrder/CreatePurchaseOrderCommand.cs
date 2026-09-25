namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.CreatePurchaseOrder;

public sealed record CreatePurchaseOrderCommand(
    Guid OrganisationId,
    Guid SupplierId,
    string Reference,
    IReadOnlyList<CreatePurchaseOrderLineCommand> Lines,
    Guid CreatedBy);
