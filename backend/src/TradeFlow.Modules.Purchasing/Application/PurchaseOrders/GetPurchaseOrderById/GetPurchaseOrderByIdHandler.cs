using Microsoft.EntityFrameworkCore;
using TradeFlow.BuildingBlocks.Exceptions;
using TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

namespace TradeFlow.Modules.Purchasing.Application.PurchaseOrders.GetPurchaseOrderById;

public sealed class GetPurchaseOrderByIdHandler(PurchasingDbContext dbContext)
{
    public async Task<GetPurchaseOrderByIdResult> HandleAsync(
        Guid organisationId,
        Guid purchaseOrderId,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty || purchaseOrderId == Guid.Empty)
        {
            throw new RequestValidationException(
                "GET_PURCHASE_ORDER_IDENTIFIER_REQUIRED",
                "Organisation and purchase order identifiers are required.");
        }

        var order = await dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(item => item.Lines)
            .SingleOrDefaultAsync(
                item => item.OrganisationId == organisationId && item.Id == purchaseOrderId,
                cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(
                "PURCHASE_ORDER_NOT_FOUND",
                "The purchase order was not found.");
        }

        var supplier = await dbContext.Suppliers.AsNoTracking()
            .SingleAsync(item => item.Id == order.SupplierId, cancellationToken);

        List<GetPurchaseOrderLineResult> lines = order.Lines
            .OrderBy(line => line.ProductSku)
            .Select(line => new GetPurchaseOrderLineResult(
                line.Id,
                line.ProductId,
                line.ProductSku,
                line.ProductName,
                line.Quantity,
                line.UnitPrice,
                line.LineTotal))
            .ToList();

        return new GetPurchaseOrderByIdResult(
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
}
