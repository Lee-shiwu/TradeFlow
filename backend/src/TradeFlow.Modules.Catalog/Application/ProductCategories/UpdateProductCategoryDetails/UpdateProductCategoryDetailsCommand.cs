namespace TradeFlow.Modules.Catalog.Application.ProductCategories.UpdateProductCategoryDetails;

public sealed record UpdateProductCategoryDetailsCommand
(
    Guid OrganisationId,
    Guid ProductCategoryId,
    string Name,
    string? Description,
    Guid ModifiedBy,
    byte[] RowVersion
);
