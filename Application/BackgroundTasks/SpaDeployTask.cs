using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.BackgroundTasks
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

            var enableTime = DateTime.UtcNow;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (enableTime < DateTime.UtcNow.AddHours(-3))
                {
                    IsEnabled = false;
                }
                
                if (!IsEnabled)
                {
                    await WaitForActivation(cancellationToken);
                    enableTime = DateTime.UtcNow;
                }
                
                
                if (SpaDeployService.HasChanged())
                {
                    IsRunning = true;
                }

                try
                {
                    await spaDeployService!.Deploy();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }

                IsRunning = false;
                

                await Sleep(cancellationToken);
            }
        }
        
        private async Task WaitForActivation(CancellationToken cancellationToken)
        {
            var waitTask = Task.Run(function: async () =>
            {
                while (!IsEnabled)
                {
                    await Task.Delay(millisecondsDelay: 5000, cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                }
            }, cancellationToken: cancellationToken);

            await Task.WhenAny(task1: waitTask, task2: Task.Delay(millisecondsDelay: -1, cancellationToken: cancellationToken));
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