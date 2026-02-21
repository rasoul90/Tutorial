using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Infrastructure.Persistence;
using DeliverySaaS.Infrastructure.Repositories;
using DeliverySaaS.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeliverySaaS.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IAccountingRepository, AccountingRepository>();
        services.AddScoped<IIntegrationRepository, IntegrationRepository>();
        services.AddSingleton<IHmacSignatureService, HmacSignatureService>();

        return services;
    }
}
