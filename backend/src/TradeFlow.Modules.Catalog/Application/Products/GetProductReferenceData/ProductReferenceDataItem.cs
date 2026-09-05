namespace TradeFlow.Modules.Catalog.Application.Products.GetProductReferenceData;

public sealed record ProductReferenceDataItem(
    Guid Id,
    string Code,
    string Name);
