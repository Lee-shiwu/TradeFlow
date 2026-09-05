namespace TradeFlow.Api.Endpoints.Catalog.Products;

public sealed record ProductReferenceDataItemResponse(
    Guid Id,
    string Code,
    string Name);
