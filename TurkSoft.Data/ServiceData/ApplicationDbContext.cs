using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Data.ServiceData
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet'ler
        public DbSet<Tm.Contact> Contacts { get; set; }
        // Diğer DbSet'ler...

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfigürasyonları uygula
            modelBuilder.ApplyConfiguration(new GibConfiguration());

            // Seed data için
            modelBuilder.Entity<Tm.Contact>().HasData(SeedData.GetContacts());
        }
    }
}
