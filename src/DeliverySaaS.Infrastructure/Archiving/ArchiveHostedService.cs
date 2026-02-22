using DeliverySaaS.Application.Archiving;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeliverySaaS.Infrastructure.Archiving;

public class ArchiveHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ArchiveHostedService> _logger;

    public ArchiveHostedService(IServiceScopeFactory scopeFactory, ILogger<ArchiveHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var next = now.Date.AddDays(1).AddHours(2);
            var delay = next - now;
            await Task.Delay(delay, stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IArchiveService>();
                var count = await svc.RunArchiveAsync(stoppingToken);
                _logger.LogInformation("Archive run completed. Archived={Count}", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Archive job failed.");
            }
        }
    }
}
