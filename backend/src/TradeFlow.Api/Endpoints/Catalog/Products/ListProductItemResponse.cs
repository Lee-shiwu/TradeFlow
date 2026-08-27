using System.Text.Json.Serialization;
using TradeFlow.Modules.Catalog.Domain.Products;

namespace TradeFlow.Api.Endpoints.Catalog.Products;

public sealed record ListProductItemResponse
(
    Guid ProductId,
    string Sku,
    string Name,
    Guid UnitOfMeasureId,
    Guid? ProductCategoryId,
    Guid TaxCategoryId,
    [property: JsonConverter(
        typeof(JsonStringEnumConverter<ProductStatus>))]
    ProductStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastModifiedAt
    );
