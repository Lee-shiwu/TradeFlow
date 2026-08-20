namespace TradeFlow.Modules.Catalog.Application.Products.UpdateProductDetails;

public sealed record UpdateProductDetailsResult
(
    Guid ProductId,
    string Name,
    string Description,
    Guid? ProductCategoryId,
    Guid TaxCategoryId,
    DateTimeOffset? LastModifiedAt,
    Guid? LastModifiedBy,
    byte[] RowVersion
);
