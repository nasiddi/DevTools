using System;
using System.Threading;
using System.Threading.Tasks;
using DevTools.Application.Models;
using DevTools.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevTools.Application.BackgroundTasks;

public class SpaDeployTask : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public bool IsEnabled { get; set; }
    public bool IsRunning { get; set; }
    public bool RunImmediately { get; set; }
    public bool HasChanged { get; private set; } = true;
    public DateTime? LastRun { get; private set; }
    public Commit? Commit { get; private set; }

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
                
            if (!IsEnabled && !RunImmediately)
            {
                await WaitForActivation(cancellationToken);
                enableTime = DateTime.UtcNow;
            }

            IsRunning = true;

            try
            {
                var commit = await spaDeployService!.Deploy();
                if (commit != null)
                {
                    Commit = commit;
                    HasChanged = true;
                }
                else
                {
                    HasChanged = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            IsRunning = false;
            LastRun = DateTime.UtcNow;
                
            if (!RunImmediately)
            {
                await Sleep(cancellationToken);
            }
            else
            {
                RunImmediately = false;
            }
        }
    }
        
    private async Task WaitForActivation(CancellationToken cancellationToken)
    {
        var waitTask = Task.Run(function: async () =>
        {
            while (!IsEnabled && !RunImmediately)
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