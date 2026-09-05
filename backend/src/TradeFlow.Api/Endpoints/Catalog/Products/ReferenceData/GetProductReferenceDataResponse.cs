namespace TradeFlow.Api.Endpoints.Catalog.Products;

public sealed record GetProductReferenceDataResponse(
    IReadOnlyList<ProductReferenceDataItemResponse> UnitsOfMeasure,
    IReadOnlyList<ProductReferenceDataItemResponse> ProductCategories,
    IReadOnlyList<ProductReferenceDataItemResponse> TaxCategories);
