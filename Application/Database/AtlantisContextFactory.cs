using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Application.Database
{
    public class AtlantisContextFactory
    {
        public class DevToolsDbContextFactory : IDesignTimeDbContextFactory<DevToolsContext>
        {
            public DevToolsContext CreateDbContext(string[] args)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true)
                    .AddJsonFile("appsettings.Secrets.json", false)
                    .AddEnvironmentVariables()
                    .Build();

                var builder = new DbContextOptionsBuilder<DevToolsContext>();

                var connectionString = configuration
                    .GetConnectionString("Database");

                builder.UseSqlServer(connectionString,
                    x => x.MigrationsAssembly(typeof(DevToolsDbContextFactory).Assembly.FullName));



                return new DevToolsContext(builder.Options);
            }
        }
    }
}