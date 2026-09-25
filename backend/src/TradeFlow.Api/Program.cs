using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using TradeFlow.Api.Endpoints.Catalog.Products;
using TradeFlow.Api.Endpoints.Catalog.ProductCategories;
using TradeFlow.Api.Endpoints.Catalog.TaxCategories;
using TradeFlow.Api.Endpoints.Purchasing.Suppliers;
using TradeFlow.Api.Endpoints.Purchasing.PurchaseOrders;
using TradeFlow.Api.Endpoints.Purchasing.GoodsReceipts;
using TradeFlow.Api.Endpoints.Inventory;
using TradeFlow.Api.Infrastructure.Errors;
using TradeFlow.Api.Infrastructure.Health;
using TradeFlow.Api.Infrastructure.Purchasing;
using TradeFlow.BuildingBlocks.Time;
using TradeFlow.Modules.Catalog;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;
using TradeFlow.Modules.Identity;
using TradeFlow.Modules.Organisations;
using TradeFlow.Modules.Purchasing;
using TradeFlow.Modules.Purchasing.Application.PurchaseOrders;
using TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddIdentityModule();
builder.Services.AddOrganisationsModule();
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddPurchasingModule(builder.Configuration);
builder.Services.AddScoped<IProductForPurchasingReader, CatalogProductForPurchasingReader>();

string[] allowedOrigins =
    builder.Configuration.GetSection("Frontend:AllowedOrigins").Get<string[]>()
    ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<CatalogDbContext>(
        name: "sqlserver",
        tags: ["ready"]);

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<PurchasingDbContext>(
        name: "purchasing-sqlserver",
        tags: ["ready"]);

WebApplication app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");

app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = _ => false,
        ResponseWriter = HealthResponseWriter.WriteAsync,
    });

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("ready"),
        ResponseWriter = HealthResponseWriter.WriteAsync,
    });

app.MapGet(
        "/api/v1/system/info",
        (IClock clock) => Results.Ok(new
        {
            service = "TradeFlow.Api",
            version = "0.1.0",
            utcNow = clock.UtcNow,
        }))
    .WithName("GetSystemInfo");


app.MapCreateProductEndpoint();
app.MapGetProductByIdEndpoint();
app.MapListProductsEndpoint();
app.MapUpdateProductDetailsEndpoint();
app.MapDeactivateProductEndpoint();
app.MapActivateProductEndpoint();
app.MapGetProductReferenceDataEndpoint();
app.MapListProductCategoriesEndpoint();
app.MapGetProductCategoryByIdEndpoint();
app.MapCreateProductCategoryEndpoint();
app.MapUpdateProductCategoryDetailsEndpoint();
app.MapActivateProductCategoryEndpoint();
app.MapDeactivateProductCategoryEndpoint();
app.MapListTaxCategoriesEndpoint();
app.MapGetTaxCategoryByIdEndpoint();
app.MapCreateTaxCategoryEndpoint();
app.MapCreateSupplierEndpoint();
app.MapListSuppliersEndpoint();
app.MapCreatePurchaseOrderEndpoint();
app.MapListPurchaseOrdersEndpoint();
app.MapGetPurchaseOrderByIdEndpoint();
app.MapConfirmPurchaseOrderEndpoint();
app.MapReceivePurchaseOrderEndpoint();
app.MapListStockEndpoint();

app.Run();

public partial class Program;
