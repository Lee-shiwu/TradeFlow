using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Modules.Catalog.Domain.Products;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;
using TradeFlow.Modules.Purchasing.Domain.Suppliers;
using TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

namespace TradeFlow.IntegrationTests.Purchasing.PurchaseOrders;

public sealed class PurchaseOrderEndpointTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private static readonly Guid EachUnitOfMeasureId =
        Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid StandardGstTaxCategoryId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task CreateListAndGet_WithValidRequest_RoundTripsDraftOrder()
    {
        Guid organisationId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Supplier supplier = Supplier.Create(
            organisationId,
            $"SUP-{Suffix()}",
            "Integration supplier",
            DateTimeOffset.UtcNow,
            userId);
        Product product = Product.Create(
            organisationId,
            $"SKU-{Suffix()}",
            "Integration product",
            null,
            EachUnitOfMeasureId,
            null,
            StandardGstTaxCategoryId,
            DateTimeOffset.UtcNow,
            userId);

        using IServiceScope scope = factory.Services.CreateScope();
        PurchasingDbContext purchasing = scope.ServiceProvider.GetRequiredService<PurchasingDbContext>();
        CatalogDbContext catalog = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await catalog.Database.MigrateAsync();
        await purchasing.Database.MigrateAsync();
        purchasing.Suppliers.Add(supplier);
        catalog.Products.Add(product);
        await catalog.SaveChangesAsync();
        await purchasing.SaveChangesAsync();

        try
        {
            using HttpClient client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Organisation-Id", organisationId.ToString());
            client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
            string reference = $"PO-{Suffix()}";

            HttpResponseMessage createResponse = await client.PostAsJsonAsync(
                "/api/v1/purchasing/purchase-orders",
                new
                {
                    supplierId = supplier.Id,
                    reference,
                    lines = new[]
                    {
                        new { productId = product.Id, quantity = 2m, unitPrice = 5.5m },
                    },
                });

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            JsonElement created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            Guid orderId = created.GetProperty("purchaseOrderId").GetGuid();
            Assert.Equal("Draft", created.GetProperty("status").GetString());
            Assert.Equal(11m, created.GetProperty("totalAmount").GetDecimal());

            JsonElement list = await client.GetFromJsonAsync<JsonElement>(
                $"/api/v1/purchasing/purchase-orders?search={reference}&pageNumber=1&pageSize=20");
            Assert.Equal(1, list.GetProperty("totalCount").GetInt32());
            Assert.Equal(orderId, list.GetProperty("items")[0].GetProperty("purchaseOrderId").GetGuid());

            JsonElement details = await client.GetFromJsonAsync<JsonElement>(
                $"/api/v1/purchasing/purchase-orders/{orderId}");
            Assert.Equal(supplier.Code, details.GetProperty("supplierCode").GetString());
            Assert.Equal(product.Sku, details.GetProperty("lines")[0].GetProperty("productSku").GetString());
        }
        finally
        {
            purchasing.ChangeTracker.Clear();
            catalog.ChangeTracker.Clear();
            await purchasing.PurchaseOrders
                .Where(order => order.OrganisationId == organisationId)
                .ExecuteDeleteAsync();
            await purchasing.Suppliers
                .Where(item => item.OrganisationId == organisationId)
                .ExecuteDeleteAsync();
            await catalog.Products
                .Where(item => item.OrganisationId == organisationId)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task List_WithAnotherOrganisation_DoesNotReturnOrder()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        PurchasingDbContext purchasing =
            scope.ServiceProvider.GetRequiredService<PurchasingDbContext>();
        await purchasing.Database.MigrateAsync();

        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Organisation-Id", Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add("X-User-Id", Guid.NewGuid().ToString());

        JsonElement list = await client.GetFromJsonAsync<JsonElement>(
            "/api/v1/purchasing/purchase-orders?pageNumber=1&pageSize=20");

        Assert.Equal(0, list.GetProperty("totalCount").GetInt32());
    }

    private static string Suffix() => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
}
