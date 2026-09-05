using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Catalog.Application.Products.GetProductReferenceData;

namespace TradeFlow.Api.Endpoints.Catalog.Products;

public static class GetProductReferenceDataEndpoint
{
    public static IEndpointRouteBuilder MapGetProductReferenceDataEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                "/api/v1/catalog/products/reference-data",
                HandleAsync)
            .WithName("GetProductReferenceData")
            .WithTags("Catalog Products")
            .Produces<GetProductReferenceDataResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        GetProductReferenceDataHandler handler,
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

        GetProductReferenceDataQuery query =
            new(identity.OrganisationId);

        GetProductReferenceDataResult result =
            await handler.HandleAsync(query, cancellationToken);

        return Results.Ok(CreateResponse(result));
    }

    private static GetProductReferenceDataResponse CreateResponse(
        GetProductReferenceDataResult result)
    {
        return new GetProductReferenceDataResponse(
            UnitsOfMeasure: MapItems(result.UnitsOfMeasure),
            ProductCategories: MapItems(result.ProductCategories),
            TaxCategories: MapItems(result.TaxCategories));
    }

    private static IReadOnlyList<ProductReferenceDataItemResponse> MapItems(
        IReadOnlyList<ProductReferenceDataItem> items)
    {
        return items
            .Select(item =>
                new ProductReferenceDataItemResponse(
                    item.Id,
                    item.Code,
                    item.Name))
            .ToList();
    }
}
