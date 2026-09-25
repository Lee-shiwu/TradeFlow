using TradeFlow.Modules.Catalog.Domain.TaxCategories;

namespace TradeFlow.Modules.Catalog.Application.TaxCategories.CreateTaxCategory;

public sealed record CreateTaxCategoryCommand(
    string Code,
    string Name,
    string? Description,
    decimal Rate,
    TaxTreatment Treatment,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo);
