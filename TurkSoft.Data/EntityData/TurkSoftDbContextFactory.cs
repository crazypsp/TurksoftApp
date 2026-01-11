using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using TurkSoft.Data.EntityData;

namespace TurkSoft.Data.EntityData
{
    public class TurkSoftDbContextFactory : IDesignTimeDbContextFactory<TurkSoftDbContext>
    {
        public TurkSoftDbContext CreateDbContext(string[] args)
        {
            // Get environment
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // Build config
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Get connection string
            var connectionString = config.GetConnectionString("DefaultConnection");

            // Configure DbContext
            var optionsBuilder = new DbContextOptionsBuilder<TurkSoftDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new TurkSoftDbContext(optionsBuilder.Options);
        }
    }
}