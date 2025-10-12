// TurkSoft.Data.Context/GibAppDbContext.cs
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TurkSoft.Entities.GIBEntityDB;
using TurkSoft.Data.Configuration;

namespace TurkSoft.Data.Context
{
    /// <summary>
    /// GIB veritabanı için bağımsız DbContext.
    /// - Fluent API config'leri otomatik yükler
    /// - CreatedAt/UpdatedAt alanlarını otomatik set eder (varsa)
    /// </summary>
    public class GibAppDbContext : DbContext
    {
        public GibAppDbContext(DbContextOptions<GibAppDbContext> options) : base(options) { }

        // === DbSet'ler (ZIP içerisindeki GERÇEK sınıflara göre) ===
        public DbSet<Address> Address { get; set; } = default!;
        public DbSet<Announcement> Announcement { get; set; } = default!;
        public DbSet<Bank> Bank { get; set; } = default!;
        public DbSet<Brand> Brand { get; set; } = default!;
        public DbSet<Category> Category { get; set; } = default!;
        public DbSet<City> City { get; set; } = default!;
        public DbSet<CommissionsMove> CommissionsMove { get; set; } = default!;
        public DbSet<Country> Country { get; set; } = default!;
        public DbSet<Currency> Currency { get; set; } = default!;
        public DbSet<Customer> Customer { get; set; } = default!;
        public DbSet<CustomersGroup> CustomersGroup { get; set; } = default!;
        public DbSet<CustomReport> CustomReport { get; set; } = default!;
        public DbSet<Dealer> Dealer { get; set; } = default!;
        public DbSet<Document> Document { get; set; } = default!;
        public DbSet<DocumentType> DocumentType { get; set; } = default!;
        public DbSet<ExchangeRate> ExchangeRate { get; set; } = default!;
        public DbSet<ExchangeRates> ExchangeRates { get; set; } = default!;
        public DbSet<GeneralReport> GeneralReport { get; set; } = default!;
        public DbSet<Group> Group { get; set; } = default!;
        public DbSet<Identifiers> Identifiers { get; set; } = default!;
        public DbSet<InfCode> InfCode { get; set; } = default!;
        public DbSet<Invoice> Invoice { get; set; } = default!;
        public DbSet<InvoicesDiscount> InvoicesDiscount { get; set; } = default!;
        public DbSet<InvoicesItem> InvoicesItem { get; set; } = default!;
        public DbSet<InvoicesPayment> InvoicesPayment { get; set; } = default!;
        public DbSet<InvoicesTax> InvoicesTax { get; set; } = default!;
        public DbSet<Item> Item { get; set; } = default!;
        public DbSet<ItemsCategory> ItemsCategory { get; set; } = default!;
        public DbSet<ItemsDiscount> ItemsDiscount { get; set; } = default!;
        public DbSet<ItemsExport> ItemsExport { get; set; } = default!;
        public DbSet<Log> Log { get; set; } = default!;
        public DbSet<Payment> Payment { get; set; } = default!;
        public DbSet<PaymentAccount> PaymentAccount { get; set; } = default!;
        public DbSet<PaymentType> PaymentType { get; set; } = default!;
        public DbSet<Permission> Permission { get; set; } = default!;    
        public DbSet<ProcesingReport> ProcesingReport { get; set; } = default!;
        public DbSet<Purchase> Purchase { get; set; } = default!;
        public DbSet<PurchaseItem> PurchaseItem { get; set; } = default!;
        public DbSet<Request> Request { get; set; } = default!;
        public DbSet<Returns> Returns { get; set; } = default!;
        public DbSet<Role> Role { get; set; } = default!;
        public DbSet<RolePermission> RolePermission { get; set; } = default!;
        public DbSet<ServicesProvider> ServicesProvider { get; set; } = default!;
        public DbSet<Setting> Setting { get; set; } = default!;
        public DbSet<Sgk> Sgk { get; set; } = default!;
        public DbSet<Stock> Stock { get; set; } = default!;
        public DbSet<StockMovement> StockMovement { get; set; } = default!;
        public DbSet<Supplier> Supplier { get; set; } = default!;
        public DbSet<Tax> Tax { get; set; } = default!;
        public DbSet<Tourist> Tourist { get; set; } = default!;
        public DbSet<Unit> Unit { get; set; } = default!;
        public DbSet<User> User { get; set; } = default!;
        public DbSet<UserAnnouncementRead> UserAnnouncementRead { get; set; } = default!;
        public DbSet<UserRole> UserRole { get; set; } = default!;
        public DbSet<Users> Users { get; set; } = default!;
        public DbSet<Warehouse> Warehouse { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Tüm configuration sınıflarını otomatik uygula
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GibAppDbContext).Assembly);
        }

        public override int SaveChanges()
        {
            ApplyAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditFields()
        {
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    var ca = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                    if (ca != null && (ca.CurrentValue == null || (DateTime)ca.CurrentValue == default))
                        ca.CurrentValue = utcNow;

                    var ua = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                    if (ua != null)
                        ua.CurrentValue = utcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    var ua = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                    if (ua != null)
                        ua.CurrentValue = utcNow;
                }
            }
        }
    }
}
