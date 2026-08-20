namespace TradeFlow.Api.Endpoints.Catalog.Products;

public sealed record UpdateProductDetailsRequest
(
  string Name,
  string? Description,
  Guid? ProductCategoryId,
  Guid TaxCategoryId,
  string RowVersion

  );
