namespace TradeFlow.Api.Endpoints.Catalog.Products;

public sealed record CreateProductRequest(
    string Sku,
    string Name,
    string? Description,
    Guid UnitOfMeasureId,
    Guid? ProductCategoryId,
    Guid TaxCategoryId
    );
