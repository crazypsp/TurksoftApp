using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TurkSoft.Entities.GIBEntityDB;
using TurkSoft.Data.Configuration;

namespace TurkSoft.Data.GibData
{
    /// <summary>
    /// GIB veritabanı DbContext'i.
    /// - CreatedAt / UpdatedAt alanlarını otomatik set eder.
    /// - Cascade delete (multiple path) hatalarına karşı tüm ilişkiler Restrict yapılmıştır.
    /// - UserAnnouncementRead ilişkisi özel olarak NoAction olarak ayarlanmıştır.
    /// - İlk migration sırasında Role ve User tabloları seed edilir.
    /// </summary>
    public class GibAppDbContext : DbContext
    {
        public GibAppDbContext(DbContextOptions<GibAppDbContext> options) : base(options) { }

        // === DbSet Tanımları ===
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
        public DbSet<ApiRefreshToken> ApiRefreshToken { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GibAppDbContext).Assembly);

            // === Cascade Delete hatalarını engelle ===
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                fk.DeleteBehavior = DeleteBehavior.Restrict;

            // === Özel: UserAnnouncementRead -> User ===
            modelBuilder.Entity<UserAnnouncementRead>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserAnnouncementReads)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // === UserRole Mapping ===
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // === SEED DATA ===
            SeedRoles(modelBuilder);
            SeedUsers(modelBuilder);
            SeedUserRoles(modelBuilder);
        }

        private void SeedRoles(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2025, 1, 1);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", Desc = "Sistem yöneticisi", CreatedAt = seedDate },
                new Role { Id = 2, Name = "Bayi", Desc = "Bayi kullanıcısı", CreatedAt = seedDate },
                new Role { Id = 3, Name = "MaliMüşavir", Desc = "Mali müşavir kullanıcısı", CreatedAt = seedDate },
                new Role { Id = 4, Name = "Firma", Desc = "Firma kullanıcısı", CreatedAt = seedDate }
            );
        }

        private void SeedUsers(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2025, 1, 1);

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "Admin", Email = "admin@gib.com", PasswordHash = "123456", CreatedAt = seedDate },
                new User { Id = 2, Username = "Bayi Kullanıcısı", Email = "bayi@gib.com", PasswordHash = "123456", CreatedAt = seedDate },
                new User { Id = 3, Username = "Mali Müşavir", Email = "mm@gib.com", PasswordHash = "123456", CreatedAt = seedDate },
                new User { Id = 4, Username = "Firma Kullanıcısı", Email = "firma@gib.com", PasswordHash = "123456", CreatedAt = seedDate }
            );
        }

        private void SeedUserRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = 1, UserId = 1, RoleId = 1 }, // Admin
                new UserRole { Id = 2, UserId = 2, RoleId = 2 }, // Bayi
                new UserRole { Id = 3, UserId = 3, RoleId = 3 }, // Mali Müşavir
                new UserRole { Id = 4, UserId = 4, RoleId = 4 }  // Firma
            );
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
                    var created = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                    if (created != null && (created.CurrentValue == null || (DateTime)created.CurrentValue == default))
                        created.CurrentValue = utcNow;

                    var updated = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                    if (updated != null)
                        updated.CurrentValue = utcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    var updated = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                    if (updated != null)
                        updated.CurrentValue = utcNow;
                }
            }
        }
    }
}
