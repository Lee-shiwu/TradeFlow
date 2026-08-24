namespace TradeFlow.Modules.Catalog.Application.Products.ActivateProduct;

public sealed record ActivateProductCommand
(
    Guid ProductId,
    Guid OrganisationId,
    Guid ModifiedBy,
    byte[] RowVersion
);
