using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Api.Endpoints.Purchasing.Suppliers;
using TradeFlow.Modules.Purchasing.Domain.Suppliers;
using TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Purchasing.Suppliers;

public sealed class SupplierEndpointTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Post_WithValidRequest_CreatesSupplierForCurrentIdentity()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        PurchasingDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<PurchasingDbContext>();
        await dbContext.Database.MigrateAsync();
        Guid organisationId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        using HttpClient client =
            CreateAuthenticatedClient(organisationId, userId);

        CreateSupplierResponse? body = null;

        try
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    "/api/v1/purchasing/suppliers",
                    new CreateSupplierRequest(
                        $" api-{CreateSuffix()} ",
                        "  Auckland   Office Supplies  "));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            body =
                await response.Content
                    .ReadFromJsonAsync<CreateSupplierResponse>();
            Assert.NotNull(body);
            Assert.StartsWith("API-", body.Code);
            Assert.Equal("Auckland Office Supplies", body.Name);
            Assert.Equal(SupplierStatus.Active, body.Status);
            Assert.Equal(userId, body.CreatedBy);
            Assert.NotEmpty(Convert.FromBase64String(body.RowVersion));
            Assert.Equal(
                $"/api/v1/purchasing/suppliers/{body.SupplierId}",
                response.Headers.Location?.ToString());

            Supplier? persisted =
                await dbContext.Suppliers
                    .AsNoTracking()
                    .SingleOrDefaultAsync(supplier =>
                        supplier.Id == body.SupplierId);
            Assert.NotNull(persisted);
            Assert.Equal(organisationId, persisted.OrganisationId);
        }
        finally
        {
            await DeleteOrganisationSuppliersAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task Post_WithDuplicateCode_ReturnsConflictProblem()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        PurchasingDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<PurchasingDbContext>();
        await dbContext.Database.MigrateAsync();
        Guid organisationId = Guid.NewGuid();
        string code = $"DUP-{CreateSuffix()}";
        dbContext.Suppliers.Add(
            Supplier.Create(
                organisationId,
                code,
                "Existing supplier",
                DateTimeOffset.UtcNow,
                Guid.NewGuid()));
        await dbContext.SaveChangesAsync();
        using HttpClient client =
            CreateAuthenticatedClient(organisationId, Guid.NewGuid());

        try
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    "/api/v1/purchasing/suppliers",
                    new CreateSupplierRequest(
                        code.ToLowerInvariant(),
                        "Duplicate supplier"));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            ProblemDetails? problem =
                await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(problem);
            Assert.Equal(
                "SUPPLIER_CODE_ALREADY_EXISTS",
                problem.Extensions["code"]?.ToString());
        }
        finally
        {
            await DeleteOrganisationSuppliersAsync(dbContext, organisationId);
        }
    }

    [Fact]
    public async Task Get_ReturnsOnlyCurrentOrganisationSuppliers()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        PurchasingDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<PurchasingDbContext>();
        await dbContext.Database.MigrateAsync();
        Guid organisationId = Guid.NewGuid();
        Guid otherOrganisationId = Guid.NewGuid();
        string suffix = CreateSuffix();
        dbContext.Suppliers.AddRange(
            Supplier.Create(
                organisationId,
                $"VISIBLE-{suffix}",
                "Visible supplier",
                DateTimeOffset.UtcNow,
                Guid.NewGuid()),
            Supplier.Create(
                otherOrganisationId,
                $"HIDDEN-{suffix}",
                "Hidden supplier",
                DateTimeOffset.UtcNow,
                Guid.NewGuid()));
        await dbContext.SaveChangesAsync();
        using HttpClient client =
            CreateAuthenticatedClient(organisationId, Guid.NewGuid());

        try
        {
            HttpResponseMessage response =
                await client.GetAsync(
                    $"/api/v1/purchasing/suppliers?search={suffix}"
                    + "&status=Active&pageNumber=1&pageSize=20");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            ListSuppliersResponse? body =
                await response.Content
                    .ReadFromJsonAsync<ListSuppliersResponse>();
            Assert.NotNull(body);
            ListSupplierItemResponse item = Assert.Single(body.Items);
            Assert.StartsWith("VISIBLE-", item.Code);
            Assert.Equal(SupplierStatus.Active, item.Status);
            Assert.Equal(1, body.TotalCount);
        }
        finally
        {
            await DeleteOrganisationSuppliersAsync(
                dbContext,
                organisationId,
                otherOrganisationId);
        }
    }

    [Theory]
    [InlineData("X-Organisation-Id")]
    [InlineData("X-User-Id")]
    public async Task Endpoints_WithMissingIdentityHeader_ReturnBadRequest(
        string missingHeader)
    {
        using HttpClient client = factory.CreateClient();
        AddIdentityHeadersExcept(client, missingHeader);

        HttpResponseMessage getResponse =
            await client.GetAsync("/api/v1/purchasing/suppliers");
        HttpResponseMessage postResponse =
            await client.PostAsJsonAsync(
                "/api/v1/purchasing/suppliers",
                new CreateSupplierRequest("SUPPLIER", "Supplier"));

        Assert.Equal(HttpStatusCode.BadRequest, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
    }

    private static void AddIdentityHeadersExcept(
        HttpClient client,
        string missingHeader)
    {
        if (missingHeader != "X-Organisation-Id")
        {
            client.DefaultRequestHeaders.Add(
                "X-Organisation-Id",
                Guid.NewGuid().ToString());
        }

        if (missingHeader != "X-User-Id")
        {
            client.DefaultRequestHeaders.Add(
                "X-User-Id",
                Guid.NewGuid().ToString());
        }
    }

    private HttpClient CreateAuthenticatedClient(
        Guid organisationId,
        Guid userId)
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(
            "X-Organisation-Id",
            organisationId.ToString());
        client.DefaultRequestHeaders.Add(
            "X-User-Id",
            userId.ToString());
        return client;
    }

    private static string CreateSuffix() =>
        Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

    private static async Task DeleteOrganisationSuppliersAsync(
        PurchasingDbContext dbContext,
        params Guid[] organisationIds)
    {
        dbContext.ChangeTracker.Clear();
        await dbContext.Suppliers
            .Where(supplier =>
                organisationIds.Contains(supplier.OrganisationId))
            .ExecuteDeleteAsync();
    }
}
