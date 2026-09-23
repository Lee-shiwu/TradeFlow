namespace TradeFlow.Modules.Catalog.Application.ProductCategories.CreateProductCategory;

public sealed record CreateProductCategoryCommand
(
    Guid OrganisationId,
    string Code,
    string Name,
    string? Description,
    Guid CreatedBy
);
