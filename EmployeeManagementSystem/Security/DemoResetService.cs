using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Security
{
    // The demo is shared, so visitor changes are removed regularly.
    public sealed class DemoResetService(IServiceScopeFactory scopes, ILogger<DemoResetService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(30));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = scopes.CreateScope();
                    DbInitializer.Reset(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    logger.LogError(exception, "The demo data reset failed");
                }
            }
        }
    }
}
