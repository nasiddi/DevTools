using DevTools.Application.Database;
using DevTools.Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevTools.Application;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = int.MaxValue;
        });

        services.Configure<FormOptions>(x =>
        {
            x.ValueLengthLimit = int.MaxValue;
            x.MultipartBodyLengthLimit = int.MaxValue;
        });
        
        services.AddCors(options => options.AddPolicy("Cors",
            builder =>
            {
                builder.WithExposedHeaders("Content-Disposition");
            }));

        services.AddDbContext<DevToolsContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("Database"), b => b.MigrationsAssembly("DevTools.Application")));

        services.AddScoped<ISpaDeployService, SpaDeployService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ICitadelsService, CitadelsService>();
        services.AddSingleton<QuizGameDataService>();
        services.AddControllersWithViews();
        // In production, the React files will be served from this directory
        services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
            
        app.UseStaticFiles();
        app.UseSpaStaticFiles();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action=Index}/{id?}");
        });

        app.UseSpa(spa =>
        {
            spa.Options.SourcePath = "ClientApp";

            if (env.IsDevelopment())
            {
                spa.UseReactDevelopmentServer(npmScript: "start");
            }
        });
        
        InitializeApplication(app);
    }
    
    private static void InitializeApplication(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var scopedProvider = scope.ServiceProvider;
        var bookingUpdater = scopedProvider.GetRequiredService<QuizGameDataService>();
        bookingUpdater.LoadQuizShow().GetAwaiter().GetResult();
    }
}