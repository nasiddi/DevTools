using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HueIrisColorTask = DevTools.BackgroundTasks.HueIrisColorTask;

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
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureServices(configureDelegate: services =>
                {
                    services.AddSingleton<HueIrisColorTask>();
                    services.AddHostedService(implementationFactory: p => p.GetRequiredService<HueIrisColorTask>());
                });
    }
}