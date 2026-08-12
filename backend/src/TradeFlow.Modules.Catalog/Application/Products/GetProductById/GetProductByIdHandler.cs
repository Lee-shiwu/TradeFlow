using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.Modules.Catalog.Application.Products.GetProductById;

public sealed class GetProductByIdHandler(CatalogDbContext dbContext)
{
    public async Task<GetProductByIdResult> HandleAsync(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<Product> productQuery =
            dbContext.Products
            .AsNoTracking()
            .Where(product =>
                product.Id == query.ProductId &&
                product.OrganisationId == query.OrganisationId);

        IQueryable<GetProductByIdResult> resultQuery =
            productQuery.Select(product =>
                new GetProductByIdResult(
                    ProductId: product.Id,
                    Sku: product.Sku,
                    Name: product.Name,
                    Description: product.Description,
                    UnitOfMeasureId: product.UnitOfMeasureId,
                    ProductCategoryId: product.ProductCategoryId,
                    TaxCategoryId: product.TaxCategoryId,
                    Status: product.Status,
                    CreatedAt: product.CreatedAt,
                    CreatedBy: product.CreatedBy,
                    LastModifiedAt: product.LastModifiedAt,
                    LastModifiedBy: product.LastModifiedBy,
                    RowVersion: product.RowVersion
                    ));
        GetProductByIdResult? result = await resultQuery.SingleOrDefaultAsync(cancellationToken);
        if (result is null) {
            throw new NotFoundException(GetProductByIdErrors.ProductNotFoundCode, GetProductByIdErrors.ProductNotFoundMessage);
        }
        return result;

    }

}
