using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace StudentProjectsCenter.Services
{
    public class StatusUpdateBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer _timer = null!;

        public StatusUpdateBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Start a timer that calls UpdateStatuses periodically
            _timer = new Timer(UpdateStatuses, stoppingToken, TimeSpan.Zero, TimeSpan.FromMinutes(30));
            await Task.CompletedTask;
        }

        private async void UpdateStatuses(object? state)
        {
            // Create a new scope for resolving scoped services
            using (var scope = _scopeFactory.CreateScope())
            {
                var statusUpdateService = scope.ServiceProvider.GetRequiredService<StatusUpdateService>();

                if (state is CancellationToken cancellationToken && cancellationToken.IsCancellationRequested)
                    return;

                // Perform status update logic
                await statusUpdateService.UpdateTaskStatusAsync();
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return base.StopAsync(cancellationToken);
        }
    }

}
