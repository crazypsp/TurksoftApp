using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TurkSoft.Data.Context;

namespace TurkSoft.Data
{
    public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>();
            options.UseSqlServer(
                "Server=localhost;Database=TurksoftDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;",
                b => b.MigrationsAssembly("TurkSoft.Data")
            );
            return new AppDbContext(options.Options);
        }
    }
}
