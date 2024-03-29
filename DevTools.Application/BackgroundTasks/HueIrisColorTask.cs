﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevTools.Application.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.HSB;

namespace DevTools.Application.BackgroundTasks;

public class HueIrisColorTask : IHostedService
{
    public bool ColorChanged { get; set; }
        
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public bool IsEnabled { get; set; }
        
    public HueIrisColorTask(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;

    }
        
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var task = ExecuteAsync(cancellationToken: cancellationToken);
            
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        IsEnabled = (await dbContext.Flags.SingleAsync(f => f.Name == "HueColorsTaskEnabled", cancellationToken: cancellationToken)).Flag;
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!IsEnabled)
            {
                await WaitForActivation(cancellationToken);
            }
                
            try
            {
                var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
                    .GetRequiredService<DevToolsContext>();
                var hueColors = await dbContext.HueColors.ToListAsync(cancellationToken);

                var locator = new HttpBridgeLocator();
                var bridges = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(5));
                var client = new LocalHueClient(bridges.First().IpAddress);
                var appKey = _configuration.GetSection("HueAppKey");
                client.Initialize(appKey.Value);
                var lights = (await client.GetLightsAsync()).ToImmutableList();

                foreach (var hueColor in hueColors)
                {
                    var lightCommand = new LightCommand();
                    lightCommand.SetColor(new RGBColor(hueColor.Color));
                    var single = lights.Single(l => l.Id == hueColor.HueId.ToString());
                    lightCommand.Brightness = single.State.Brightness;
                    await client.SendCommandAsync(lightCommand, lightList: new List<string> {hueColor.HueId.ToString()});
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            ColorChanged = false;
            await Sleep(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)  
    {
        return Task.CompletedTask;
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

    private async Task Sleep(CancellationToken cancellationToken)
    {
        var waitTask = Task.Run(function: async () =>
        {
            while (!ColorChanged)
            {
                await Task.Delay(millisecondsDelay: 200, cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
        }, cancellationToken: cancellationToken);

        await Task.WhenAny(task1: waitTask, task2: Task.Delay(millisecondsDelay: 5000, cancellationToken: cancellationToken));
    }
}