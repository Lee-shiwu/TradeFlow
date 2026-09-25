using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Modules.Purchasing.Application.Suppliers.CreateSupplier;
using TradeFlow.Modules.Purchasing.Application.Suppliers.ListSuppliers;
using TradeFlow.Modules.Purchasing.Infrastructure.Persistence;

namespace TradeFlow.Modules.Purchasing;

public static class PurchasingModule
{
    public static IServiceCollection AddPurchasingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PurchasingDbContext>(options =>
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
                    sqlServerOptions.MigrationsAssembly(
                        typeof(PurchasingDbContext).Assembly.FullName);
                    sqlServerOptions.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        "purchasing");
                    sqlServerOptions.EnableRetryOnFailure();
                });
        });

        services.AddScoped<CreateSupplierHandler>();
        services.AddScoped<ListSuppliersHandler>();

        return services;
    }
}
