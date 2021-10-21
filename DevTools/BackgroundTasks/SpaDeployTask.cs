using System.Threading;
using System.Threading.Tasks;
using DevTools.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevTools.BackgroundTasks
{
    public class SpaDeployTask : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public bool IsEnabled { get; set; } = true;
        public bool IsRunning { get; set; }

        public SpaDeployTask(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var task = ExecuteAsync(cancellationToken: cancellationToken);
            return task.IsCompleted ? task : Task.CompletedTask;
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var spaDeployService = scope.ServiceProvider.GetService<ISpaDeployService>();

            while (!cancellationToken.IsCancellationRequested)
            {
                if (IsEnabled)
                {
                    if (SpaDeployService.HasChanged())
                    {
                        IsRunning = true;
                    }

                    await spaDeployService!.Deploy();
                    IsRunning = false;
                }

                await Sleep(cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)  
        {
            return Task.CompletedTask;
        }

        private async Task Sleep(CancellationToken cancellationToken)
        {
            await Task.Delay(millisecondsDelay: 5000, cancellationToken: cancellationToken);
        }
    }
}