using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;

namespace TradeFlow.Modules.Catalog.Application.Products.DeactivateProduct;

public sealed class DeactivateProductHandler(CatalogDbContext dbContext, IClock clock) 
{
  public async Task<DeactivateProductResult> HandleAsync(
      DeactivateProductCommand command ,
      CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommand(command);

        Product? product = await dbContext.Products
            .SingleOrDefaultAsync(existingProduct =>
            existingProduct.OrganisationId == command.OrganisationId &&
            existingProduct.Id == command.ProductId,
            cancellationToken);
        if(product is null)
        {
            throw new NotFoundException(DeactivateProductErrors.ProductNotFoundCode, DeactivateProductErrors.ProductNotFoundMessage);
        }


        DateTimeOffset modifiedAt = clock.UtcNow;

        var rowVersionEntry = dbContext.Entry(product)
            .Property(existingProduct =>
            existingProduct.RowVersion);

        rowVersionEntry.OriginalValue = command.RowVersion;

        product.Deactivate(
            modifiedAt: modifiedAt,
            modifiedBy: command.ModifiedBy);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {

            throw new BusinessRuleException(DeactivateProductErrors.ConcurrencyConflictCode, DeactivateProductErrors.ConcurrencyConflictMessage);
        }

        return new DeactivateProductResult(
            ProductId: product.Id,
            Status: product.Status,
            LastModifiedAt: product.LastModifiedAt,
            LastModifiedBy: product.LastModifiedBy,
            RowVersion: product.RowVersion
            );


    }
    private static void ValidateCommand(DeactivateProductCommand command)
    {
        if (command.OrganisationId == Guid.Empty)
        {
            throw new RequestValidationException(DeactivateProductErrors.OrganisationRequiredCode, DeactivateProductErrors.OrganisationRequiredMessage);
        }
        if(command.ProductId == Guid.Empty)
        {
            throw new RequestValidationException(DeactivateProductErrors.ProductIdRequiredCode, DeactivateProductErrors.ProductIdRequiredMessage);
        }

        if(command.ModifiedBy == Guid.Empty)
        {
            throw new RequestValidationException(DeactivateProductErrors.ModifiedByRequiredCode, DeactivateProductErrors.ModifiedByRequiredMessage);
        }

        if(command.RowVersion == null || command.RowVersion.Length == 0)
        {
            throw new RequestValidationException(DeactivateProductErrors.RowVersionRequiredCode, DeactivateProductErrors.RowVersionRequiredMessage);
        }

    }

    
}
