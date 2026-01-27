using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TurkSoft.Data.EntityData;

namespace TurkSoft.Data
{
    public sealed class TurkSoftDbContextFactory
        : IDesignTimeDbContextFactory<TurkSoftDbContext>
    {
        public TurkSoftDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<TurkSoftDbContext>();

            options.UseSqlServer(
                "Server=TEKNIKSRV2;Database=TigerBankDB;User ID=sa;Password=AdanusLogo1;TrustServerCertificate=True;Encrypt=False",
                b => b.MigrationsAssembly("TurkSoft.Data")
            );

            return new TurkSoftDbContext(options.Options);
        }
    }
}
