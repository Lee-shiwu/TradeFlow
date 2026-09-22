using TradeFlow.Api.Infrastructure.Identity;
using TradeFlow.Modules.Catalog.Application.ProductCategories.ListProductCategories;

namespace TradeFlow.Api.Endpoints.Catalog.ProductCategories;

public static class ListProductCategoriesEndpoint
{
    public static IEndpointRouteBuilder MapListProductCategoriesEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                "/api/v1/catalog/product-categories",
                HandleAsync)
            .WithName("ListProductCategories")
            .WithTags("Catalog Product Categories")
            .Produces<ListProductCategoriesResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] ListProductCategoriesRequest request,
        HttpContext httpContext,
        ListProductCategoriesHandler handler,
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

        ListProductCategoriesQuery query =
            CreateQuery(request, identity.OrganisationId);

        ListProductCategoriesResult result =
            await handler.HandleAsync(query, cancellationToken);

        return Results.Ok(CreateResponse(result));
    }

    private static ListProductCategoriesQuery CreateQuery(
        ListProductCategoriesRequest request,
        Guid organisationId)
    {
        return new ListProductCategoriesQuery(
            OrganisationId: organisationId,
            Search: request.Search,
            Status: request.Status,
            PageNumber: request.PageNumber ?? 1,
            PageSize: request.PageSize ?? 20);
    }

    private static ListProductCategoriesResponse CreateResponse(
        ListProductCategoriesResult result)
    {
        List<ListProductCategoryItemResponse> items =
            result.Items
                .Select(item =>
                    new ListProductCategoryItemResponse(
                        ProductCategoryId: item.ProductCategoryId,
                        Code: item.Code,
                        Name: item.Name,
                        Description: item.Description,
                        Status: item.Status,
                        CreatedAt: item.CreatedAt,
                        LastModifiedAt: item.LastModifiedAt))
                .ToList();

        return new ListProductCategoriesResponse(
            Items: items,
            PageNumber: result.PageNumber,
            PageSize: result.PageSize,
            TotalCount: result.TotalCount,
            TotalPages: result.TotalPages);
    }
}
