using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Printing;
using DeliverySaaS.Application.Orders;
using DeliverySaaS.Infrastructure.Persistence;
using DeliverySaaS.Infrastructure.Repositories;
using DeliverySaaS.Infrastructure.Printing;
using DeliverySaaS.Infrastructure.Security;
using DeliverySaaS.Infrastructure.Caching;
using DeliverySaaS.Application.Reports;
using DeliverySaaS.Infrastructure.QueryServices;
using DeliverySaaS.Infrastructure.Archiving;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DeliverySaaS.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddMemoryCache();

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IAccountingRepository, AccountingRepository>();
        services.AddScoped<IIntegrationRepository, IntegrationRepository>();
        services.AddScoped<IArchiveRepository, ArchiveRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IMerchantPaymentsRepository, MerchantPaymentsRepository>();
        services.AddSingleton<IHmacSignatureService, HmacSignatureService>();
        services.AddSingleton<IReplayProtectionService, ReplayProtectionService>();
        services.AddScoped<IReferenceDataCacheService, ReferenceDataCacheService>();
        services.AddScoped<ILabelService, LabelService>();
        services.AddScoped<IFinancialReportsQueryService, FinancialReportsQueryService>();
        services.AddScoped<IMerchantStatementsQueryService, MerchantStatementsQueryService>();
        services.AddScoped<IDeliveryAgentStatementsQueryService, DeliveryAgentStatementsQueryService>();
        services.AddScoped<IMerchantLedgerQueryService, MerchantLedgerQueryService>();
        services.AddScoped<IProfitReportsQueryService, ProfitReportsQueryService>();
        services.AddHostedService<ArchiveHostedService>();
        services.AddScoped<IOrderSearchQueryService, OrderSearchQueryService>();

        return services;
    }
}
