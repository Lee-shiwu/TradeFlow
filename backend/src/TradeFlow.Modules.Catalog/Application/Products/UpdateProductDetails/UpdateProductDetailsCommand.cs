namespace TradeFlow.Modules.Catalog.Application.Products.UpdateProductDetails;

public sealed record UpdateProductDetailsCommand
(
    Guid OrganisationId,
    Guid ProductId,
    string Name,
    string Description,
    Guid? ProductCategoryId,
    Guid TaxCategoryId,
    Guid ModifiedBy,
    byte[] RowVersion
);
