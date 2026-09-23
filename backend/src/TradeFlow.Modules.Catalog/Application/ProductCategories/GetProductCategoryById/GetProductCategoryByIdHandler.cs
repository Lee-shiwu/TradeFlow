using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.Modules.Catalog.Application.ProductCategories.GetProductCategoryById;

public sealed class GetProductCategoryByIdHandler(CatalogDbContext dbContext)
{
    public async Task<GetProductCategoryByIdResult> HandleAsync(
        GetProductCategoryByIdQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        GetProductCategoryByIdResult? result =
            await dbContext.ProductCategories
                .AsNoTracking()
                .Where(category =>
                    category.OrganisationId == query.OrganisationId
                    && category.Id == query.ProductCategoryId)
                .Select(category =>
                    new GetProductCategoryByIdResult(
                        ProductCategoryId: category.Id,
                        Code: category.Code,
                        Name: category.Name,
                        Description: category.Description,
                        Status: category.Status,
                        CreatedAt: category.CreatedAt,
                        CreatedBy: category.CreatedBy,
                        LastModifiedAt: category.LastModifiedAt,
                        LastModifiedBy: category.LastModifiedBy,
                        RowVersion: category.RowVersion))
                .SingleOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            throw new NotFoundException(
                GetProductCategoryByIdErrors.ProductCategoryNotFoundCode,
                GetProductCategoryByIdErrors.ProductCategoryNotFoundMessage);
        }

        return result;
    }
}
