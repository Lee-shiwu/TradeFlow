using System.Text.Json.Serialization;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;

namespace TradeFlow.Api.Endpoints.Catalog.TaxCategories;

public sealed record GetTaxCategoryByIdResponse(
    Guid TaxCategoryId,
    string Code,
    string Name,
    string Description,
    decimal Rate,
    [property: JsonConverter(
        typeof(JsonStringEnumConverter<TaxTreatment>))]
    TaxTreatment Treatment,
    [property: JsonConverter(
        typeof(JsonStringEnumConverter<TaxCategoryStatus>))]
    TaxCategoryStatus Status,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string RowVersion);
