using System.Text.Json.Serialization;
using TradeFlow.Modules.Catalog.Domain.Products;

namespace TradeFlow.Api.Endpoints.Catalog.Products;

public sealed record GetProductByIdResponse
(
    Guid ProductId,
    string Sku,
    string Name,
    string Description,
    Guid UnitOfMeasureId,
    Guid? ProductCategoryId,
    Guid TaxCategoryId,
    [property: JsonConverter(
        typeof(JsonStringEnumConverter<ProductStatus>))]
    ProductStatus Status,
    DateTimeOffset CreatedAt,
    Guid CreatedBy,
    DateTimeOffset? LastModifiedAt,
    Guid? LastModifiedBy,
    string RowVersion
    );
