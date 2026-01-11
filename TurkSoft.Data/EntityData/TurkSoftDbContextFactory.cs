using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using TurkSoft.Data.EntityData;

namespace TurkSoft.Data
{
    public sealed class TurkSoftDbContextFactory : IDesignTimeDbContextFactory<TurkSoftDbContext>
    {
        public TurkSoftDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<TurkSoftDbContext>();

            // DESKTOP-54QF28R\ZRV2014EXP için connection string
            var cs = Environment.GetEnvironmentVariable("TURKSOFT_CONNECTION_STRING")
                    ?? @"Server=DESKTOP-54QF28R\ZRV2014EXP;Database=TigerBankDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

            options.UseSqlServer(cs, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("TurkSoft.Data");
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });

            return new TurkSoftDbContext(options.Options);
        }
    }
}