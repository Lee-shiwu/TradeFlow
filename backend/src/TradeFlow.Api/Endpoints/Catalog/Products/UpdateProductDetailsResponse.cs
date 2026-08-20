namespace TradeFlow.Api.Endpoints.Catalog.Products;

public sealed record UpdateProductDetailsResponse
(
    Guid ProductId,
    string Name,
    string Description,
    Guid? ProductCategoryId,
    Guid TaxCategoryId,
    DateTimeOffset? LastModifiedAt,
    Guid? LastModifiedBy,
    string RowVersion
    );
