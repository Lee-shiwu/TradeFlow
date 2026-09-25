using TradeFlow.Modules.Catalog.Domain.TaxCategories;

namespace TradeFlow.Modules.Catalog.Application.TaxCategories.CreateTaxCategory;

public sealed record CreateTaxCategoryResult(
    Guid TaxCategoryId,
    string Code,
    string Name,
    string Description,
    decimal Rate,
    TaxTreatment Treatment,
    TaxCategoryStatus Status,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    byte[] RowVersion);
