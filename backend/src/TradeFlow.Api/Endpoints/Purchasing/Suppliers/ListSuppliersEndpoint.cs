using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Purchasing.Application.Suppliers.ListSuppliers;

namespace TradeFlow.Api.Endpoints.Purchasing.Suppliers;

public static class ListSuppliersEndpoint
{
    public static IEndpointRouteBuilder MapListSuppliersEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/purchasing/suppliers", HandleAsync)
            .WithName("ListSuppliers")
            .WithTags("Purchasing Suppliers")
            .Produces<ListSuppliersResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] ListSuppliersRequest request,
        HttpContext httpContext,
        ListSuppliersHandler handler,
        IHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        IResult? identityError =
            TemporaryIdentityResolver.TryResolve(
                httpContext,
                environment,
                out TemporaryIdentity identity);

        if (identityError is not null)
        {
            return identityError;
        }

        ListSuppliersResult result =
            await handler.HandleAsync(
                new ListSuppliersQuery(
                    identity.OrganisationId,
                    request.Search,
                    request.Status,
                    request.PageNumber ?? 1,
                    request.PageSize ?? 20),
                cancellationToken);

        List<ListSupplierItemResponse> items =
            result.Items
                .Select(item =>
                    new ListSupplierItemResponse(
                        item.SupplierId,
                        item.Code,
                        item.Name,
                        item.Status,
                        item.CreatedAt))
                .ToList();

        return Results.Ok(
            new ListSuppliersResponse(
                items,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                result.TotalPages));
    }
}
