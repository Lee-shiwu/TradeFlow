using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Catalog.Application.TaxCategories.GetTaxCategoryById;

namespace TradeFlow.Api.Endpoints.Catalog.TaxCategories;

public static class GetTaxCategoryByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetTaxCategoryByIdEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                "/api/v1/catalog/tax-categories/{taxCategoryId:guid}",
                HandleAsync)
            .WithName("GetTaxCategoryById")
            .WithTags("Catalog Tax Categories")
            .Produces<GetTaxCategoryByIdResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid taxCategoryId,
        HttpContext httpContext,
        GetTaxCategoryByIdHandler handler,
        IHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        IResult? identityError =
            TemporaryIdentityResolver.TryResolve(
                httpContext,
                environment,
                out _);

        if (identityError is not null)
        {
            return identityError;
        }

        GetTaxCategoryByIdResult result =
            await handler.HandleAsync(
                new GetTaxCategoryByIdQuery(taxCategoryId),
                cancellationToken);

        return Results.Ok(CreateResponse(result));
    }

    private static GetTaxCategoryByIdResponse CreateResponse(
        GetTaxCategoryByIdResult result)
    {
        return new GetTaxCategoryByIdResponse(
            result.TaxCategoryId,
            result.Code,
            result.Name,
            result.Description,
            result.Rate,
            result.Treatment,
            result.Status,
            result.EffectiveFrom,
            result.EffectiveTo,
            Convert.ToBase64String(result.RowVersion));
    }
}
