using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;
using TradeFlow.Modules.Purchasing.Domain.Suppliers;
using TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderHandler(
    PurchasingDbContext dbContext,
    IProductForPurchasingReader productReader,
    IClock clock)
{
    private const string ReferenceUniqueIndexName =
        "UX_PurchaseOrders_OrganisationId_Reference";

    public async Task<CreatePurchaseOrderResult> HandleAsync(
        CreatePurchaseOrderCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        Supplier? supplier =
            await dbContext.Suppliers.SingleOrDefaultAsync(
                item =>
                    item.Id == command.SupplierId
                    && item.OrganisationId == command.OrganisationId,
                cancellationToken);

        if (supplier is null)
        {
            throw new NotFoundException(
                CreatePurchaseOrderErrors.SupplierNotFoundCode,
                CreatePurchaseOrderErrors.SupplierNotFoundMessage);
        }

        if (supplier.Status != SupplierStatus.Active)
        {
            throw new BusinessRuleException(
                CreatePurchaseOrderErrors.SupplierInactiveCode,
                CreatePurchaseOrderErrors.SupplierInactiveMessage);
        }

        IReadOnlyList<CreatePurchaseOrderLineCommand> requestedLines = command.Lines ?? [];
        Guid[] productIds = requestedLines.Select(line => line.ProductId).Distinct().ToArray();
        IReadOnlyDictionary<Guid, ProductForPurchasing> products =
            await productReader.GetByIdsAsync(
                command.OrganisationId,
                productIds,
                cancellationToken);

        if (products.Count != productIds.Length)
        {
            throw new NotFoundException(
                CreatePurchaseOrderErrors.ProductNotFoundCode,
                CreatePurchaseOrderErrors.ProductNotFoundMessage);
        }

        if (products.Values.Any(product => !product.IsActive))
        {
            throw new BusinessRuleException(
                CreatePurchaseOrderErrors.ProductInactiveCode,
                CreatePurchaseOrderErrors.ProductInactiveMessage);
        }

        List<PurchaseOrderLineDraft> drafts =
            requestedLines.Select(line =>
                {
                    ProductForPurchasing product = products[line.ProductId];
                    return new PurchaseOrderLineDraft(
                        product.ProductId,
                        product.Sku,
                        product.Name,
                        line.Quantity,
                        line.UnitPrice);
                })
                .ToList();

        PurchaseOrder order =
            PurchaseOrder.Create(
                command.OrganisationId,
                command.SupplierId,
                command.Reference,
                drafts,
                clock.UtcNow,
                command.CreatedBy);

        bool referenceExists =
            await dbContext.PurchaseOrders.AnyAsync(
                existingOrder =>
                    existingOrder.OrganisationId == order.OrganisationId
                    && existingOrder.Reference == order.Reference,
                cancellationToken);

        if (referenceExists)
        {
            throw CreateDuplicateReferenceException();
        }

        dbContext.PurchaseOrders.Add(order);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsDuplicateReferenceException(exception))
        {
            throw CreateDuplicateReferenceException();
        }

        return MapResult(order, supplier);
    }

    private static CreatePurchaseOrderResult MapResult(
        PurchaseOrder order,
        Supplier supplier)
    {
        List<CreatePurchaseOrderLineResult> lines =
            order.Lines.Select(line =>
                new CreatePurchaseOrderLineResult(
                    line.Id,
                    line.ProductId,
                    line.ProductSku,
                    line.ProductName,
                    line.Quantity,
                    line.UnitPrice,
                    line.LineTotal))
                .ToList();

        return new CreatePurchaseOrderResult(
            order.Id,
            order.SupplierId,
            supplier.Code,
            supplier.Name,
            order.Reference,
            order.Status,
            lines,
            order.TotalAmount,
            order.CreatedAt,
            order.CreatedBy,
            order.RowVersion);
    }

    private static BusinessRuleException CreateDuplicateReferenceException() =>
        new(
            CreatePurchaseOrderErrors.ReferenceAlreadyExistsCode,
            CreatePurchaseOrderErrors.ReferenceAlreadyExistsMessage);

    private static bool IsDuplicateReferenceException(DbUpdateException exception) =>
        exception.InnerException is SqlException sqlException
        && sqlException.Number is 2601 or 2627
        && sqlException.Message.Contains(
            ReferenceUniqueIndexName,
            StringComparison.OrdinalIgnoreCase);
}
