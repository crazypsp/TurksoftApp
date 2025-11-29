using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TurkSoft.Data.GibData
{
    /// <summary>
    /// Sadece design-time (migration) için kullanılan DbContext factory'si.
    /// Runtime'da DI ile kayıtlı olan GibAppDbContext kullanılmaya devam eder.
    /// </summary>
    public class GibAppDbContextFactory : IDesignTimeDbContextFactory<GibAppDbContext>
    {
        public GibAppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GibAppDbContext>();

            // !!! Burayı kendi kullanıcı/şifrene göre düzenle
            var connectionString =
                "Server=213.159.6.252,1433;" +
                "Database=TurkSoftGibDB;" +
                "Trusted_Connection=False;" +              // uzak IP ise genelde False
                "User Id=sa;" +                // kendi user
                "Password=Sincap@123;" +                     // kendi şifre
                "TrustServerCertificate=True;";

            optionsBuilder.UseSqlServer(connectionString);

            return new GibAppDbContext(optionsBuilder.Options);
        }
    }
}
