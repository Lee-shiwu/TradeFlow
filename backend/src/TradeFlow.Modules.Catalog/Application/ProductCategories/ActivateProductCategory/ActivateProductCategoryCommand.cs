namespace TradeFlow.Modules.Catalog.Application.ProductCategories.ActivateProductCategory;

public sealed record ActivateProductCategoryCommand(
    Guid OrganisationId,
    Guid ProductCategoryId,
    Guid ModifiedBy,
    byte[] RowVersion);
