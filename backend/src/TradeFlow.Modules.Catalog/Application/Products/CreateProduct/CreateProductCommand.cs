namespace TradeFlow.Modules.Catalog.Application.Products.CreateProduct;

public sealed record CreateProductCommand(
    Guid OrganisationId,
    string Sku,
    string Name,
    string? Description,
    Guid UnitOfMeasureId,
    Guid? ProductCategoryId,
    Guid TaxCategoryId,
    Guid CreatedBy
);
