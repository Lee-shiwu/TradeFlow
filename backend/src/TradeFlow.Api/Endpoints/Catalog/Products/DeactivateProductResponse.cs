using System.Text.Json.Serialization;
using TradeFlow.Modules.Catalog.Domain.Products;

namespace TradeFlow.Api.Endpoints.Catalog.Products;

public sealed record DeactivateProductResponse
(
    Guid ProductId,
    [property: JsonConverter(
        typeof(JsonStringEnumConverter<ProductStatus>))]
    ProductStatus Status,
    DateTimeOffset? LastModifiedAt,
    Guid? LastModifiedBy,
    string RowVersion
    );
