using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Modules.Catalog.Infrastructure.Persistence;
using TradeFlow.Modules.Catalog.Application.Products.CreateProduct;
using TradeFlow.Modules.Catalog.Application.Products.GetProductById;

namespace TradeFlow.Modules.Catalog;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options =>
        {
            string connectionString =
                configuration.GetConnectionString("TradeFlowDatabase")
                ?? throw new InvalidOperationException(
                    "Connection string 'TradeFlowDatabase' is not configured. "
                    + "Use .NET User Secrets for local development.");

            options.UseSqlServer(
                connectionString,
                sqlServerOptions =>
                {
                    sqlServerOptions.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName);
                    sqlServerOptions.MigrationsHistoryTable("__EFMigrationsHistory", "catalog");
                    sqlServerOptions.EnableRetryOnFailure();
                });
        });

        services.AddScoped<CreateProductHandler>();
        services.AddScoped<GetProductByIdHandler>();

        return services;
    }
}
