namespace TradeFlow.Modules.Catalog.Application.Products.GetProductReferenceData;

public sealed record GetProductReferenceDataResult(
    IReadOnlyList<ProductReferenceDataItem> UnitsOfMeasure,
    IReadOnlyList<ProductReferenceDataItem> ProductCategories,
    IReadOnlyList<ProductReferenceDataItem> TaxCategories);
