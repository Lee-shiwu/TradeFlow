namespace TradeFlow.Modules.Catalog.Application.ProductCategories.DeactivateProductCategory;

public sealed record DeactivateProductCategoryCommand(
    Guid OrganisationId,
    Guid ProductCategoryId,
    Guid ModifiedBy,
    byte[] RowVersion);
