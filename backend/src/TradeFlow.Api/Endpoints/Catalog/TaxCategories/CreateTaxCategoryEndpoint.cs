using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Catalog.Application.TaxCategories.CreateTaxCategory;

namespace TradeFlow.Api.Endpoints.Catalog.TaxCategories;

public static class CreateTaxCategoryEndpoint
{
    public static IEndpointRouteBuilder MapCreateTaxCategoryEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/catalog/tax-categories", HandleAsync)
            .WithName("CreateTaxCategory")
            .WithTags("Catalog Tax Categories")
            .Accepts<CreateTaxCategoryRequest>("application/json")
            .Produces<CreateTaxCategoryResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        CreateTaxCategoryRequest request,
        HttpContext httpContext,
        CreateTaxCategoryHandler handler,
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

        CreateTaxCategoryResult result =
            await handler.HandleAsync(
                new CreateTaxCategoryCommand(
                    request.Code,
                    request.Name,
                    request.Description,
                    request.Rate,
                    request.Treatment,
                    request.EffectiveFrom,
                    request.EffectiveTo),
                cancellationToken);

        return Results.Created(
            $"/api/v1/catalog/tax-categories/{result.TaxCategoryId}",
            CreateResponse(result));
    }

    private static CreateTaxCategoryResponse CreateResponse(
        CreateTaxCategoryResult result)
    {
        return new CreateTaxCategoryResponse(
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
