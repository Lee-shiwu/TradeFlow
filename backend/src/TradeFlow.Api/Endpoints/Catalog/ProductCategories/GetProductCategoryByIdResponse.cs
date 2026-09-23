using System.Text.Json.Serialization;
using TradeFlow.Modules.Catalog.Domain.ProductCategories;

namespace TradeFlow.Api.Endpoints.Catalog.ProductCategories;

public sealed record GetProductCategoryByIdResponse
(
    Guid ProductCategoryId,
    string Code,
    string Name,
    string Description,
    [property: JsonConverter(
        typeof(JsonStringEnumConverter<ProductCategoryStatus>))]
    ProductCategoryStatus Status,
    DateTimeOffset CreatedAt,
    Guid CreatedBy,
    DateTimeOffset? LastModifiedAt,
    Guid? LastModifiedBy,
    string RowVersion
);
