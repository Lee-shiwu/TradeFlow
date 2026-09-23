namespace TradeFlow.Modules.Catalog.Application.ProductCategories.GetProductCategoryById;

public sealed record GetProductCategoryByIdQuery
(
    Guid OrganisationId,
    Guid ProductCategoryId
);
