using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HueIrisColorTask = DevTools.BackgroundTasks.HueIrisColorTask;
using SpaDeployTask = DevTools.BackgroundTasks.SpaDeployTask;

namespace DevTools
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((_, config) =>
                {
                    config.AddJsonFile(path: "appsettings.Secrets.json", optional: false, reloadOnChange: true)
                        .AddJsonFile(path: "appsettings.DevelopementMachine.json", optional: true, reloadOnChange: true);

                    config.AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://*:7000");
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(configureDelegate: services =>
                {
                    services.AddSingleton<SpaDeployTask>();
                    services.AddHostedService(implementationFactory: p => p.GetRequiredService<SpaDeployTask>());
                    services.AddSingleton<HueIrisColorTask>();
                    services.AddHostedService(implementationFactory: p => p.GetRequiredService<HueIrisColorTask>());
                });
    }
}