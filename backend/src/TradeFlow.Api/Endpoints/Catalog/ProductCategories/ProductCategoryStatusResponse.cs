using System.Text.Json.Serialization;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;

namespace TradeFlow.Api.Endpoints.Catalog.ProductCategories;

public sealed record ProductCategoryStatusResponse(
    Guid ProductCategoryId,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    ProductCategoryStatus Status,
    DateTimeOffset? LastModifiedAt,
    Guid? LastModifiedBy,
    string RowVersion);
