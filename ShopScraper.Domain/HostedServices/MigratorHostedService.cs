namespace ShopScraper.Domain.HostedServices;

using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class MigratorHostedService(IServiceProvider provider, ILogger<MigratorHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Накатываем миграции");
        await context.Database.MigrateAsync(cancellationToken: cancellationToken);
        logger.LogInformation("Накатка миграций завершена");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}