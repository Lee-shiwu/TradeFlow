using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.Modules.Catalog.Application.TaxCategories.GetTaxCategoryById;

public sealed class GetTaxCategoryByIdHandler(CatalogDbContext dbContext)
{
    public async Task<GetTaxCategoryByIdResult> HandleAsync(
        GetTaxCategoryByIdQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        GetTaxCategoryByIdResult? result =
            await dbContext.TaxCategories
                .AsNoTracking()
                .Where(category => category.Id == query.TaxCategoryId)
                .Select(category =>
                    new GetTaxCategoryByIdResult(
                        category.Id,
                        category.Code,
                        category.Name,
                        category.Description,
                        category.Rate,
                        category.Treatment,
                        category.Status,
                        category.EffectiveFrom,
                        category.EffectiveTo,
                        category.RowVersion))
                .SingleOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            throw new NotFoundException(
                GetTaxCategoryByIdErrors.TaxCategoryNotFoundCode,
                GetTaxCategoryByIdErrors.TaxCategoryNotFoundMessage);
        }

        return result;
    }
}
