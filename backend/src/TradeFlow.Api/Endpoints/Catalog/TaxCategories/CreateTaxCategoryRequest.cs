using System.Text.Json.Serialization;
using TradeFlow.Modules.Catalog.Domain.TaxCategories;

namespace TradeFlow.Api.Endpoints.Catalog.TaxCategories;

public sealed record CreateTaxCategoryRequest(
    string Code,
    string Name,
    string? Description,
    decimal Rate,
    [property: JsonConverter(
        typeof(JsonStringEnumConverter<TaxTreatment>))]
    TaxTreatment Treatment,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo);
