namespace TradeFlow.Modules.Catalog.Application.Products.DeactivateProduct;

public sealed record DeactivateProductCommand
(
    Guid OrganisationId,
    Guid ProductId,
    Guid ModifiedBy,
    byte[] RowVersion
    );
