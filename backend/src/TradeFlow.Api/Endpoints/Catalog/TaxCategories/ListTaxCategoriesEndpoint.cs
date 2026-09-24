using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Catalog.Application.TaxCategories.ListTaxCategories;

namespace TradeFlow.Api.Endpoints.Catalog.TaxCategories;

public static class ListTaxCategoriesEndpoint
{
    public static IEndpointRouteBuilder MapListTaxCategoriesEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/catalog/tax-categories", HandleAsync)
            .WithName("ListTaxCategories")
            .WithTags("Catalog Tax Categories")
            .Produces<ListTaxCategoriesResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] ListTaxCategoriesRequest request,
        HttpContext httpContext,
        ListTaxCategoriesHandler handler,
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

        ListTaxCategoriesResult result =
            await handler.HandleAsync(
                new ListTaxCategoriesQuery(
                    request.Search,
                    request.Status,
                    request.PageNumber ?? 1,
                    request.PageSize ?? 20),
                cancellationToken);

        return Results.Ok(CreateResponse(result));
    }

    private static ListTaxCategoriesResponse CreateResponse(
        ListTaxCategoriesResult result)
    {
        List<ListTaxCategoryItemResponse> items =
            result.Items
                .Select(item =>
                    new ListTaxCategoryItemResponse(
                        item.TaxCategoryId,
                        item.Code,
                        item.Name,
                        item.Description,
                        item.Rate,
                        item.Treatment,
                        item.Status,
                        item.EffectiveFrom,
                        item.EffectiveTo))
                .ToList();

        return new ListTaxCategoriesResponse(
            items,
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages);
    }
}
