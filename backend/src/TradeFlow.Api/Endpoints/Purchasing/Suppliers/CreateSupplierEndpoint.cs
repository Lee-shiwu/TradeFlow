using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Purchasing.Application.Suppliers.CreateSupplier;

namespace TradeFlow.Api.Endpoints.Purchasing.Suppliers;

public static class CreateSupplierEndpoint
{
    public static IEndpointRouteBuilder MapCreateSupplierEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/purchasing/suppliers", HandleAsync)
            .WithName("CreateSupplier")
            .WithTags("Purchasing Suppliers")
            .Accepts<CreateSupplierRequest>("application/json")
            .Produces<CreateSupplierResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        CreateSupplierRequest request,
        HttpContext httpContext,
        CreateSupplierHandler handler,
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

        CreateSupplierResult result =
            await handler.HandleAsync(
                new CreateSupplierCommand(
                    identity.OrganisationId,
                    request.Code,
                    request.Name,
                    identity.UserId),
                cancellationToken);

        CreateSupplierResponse response =
            new(
                result.SupplierId,
                result.Code,
                result.Name,
                result.Status,
                result.CreatedAt,
                result.CreatedBy,
                Convert.ToBase64String(result.RowVersion));

        return Results.Created(
            $"/api/v1/purchasing/suppliers/{result.SupplierId}",
            response);
    }
}
