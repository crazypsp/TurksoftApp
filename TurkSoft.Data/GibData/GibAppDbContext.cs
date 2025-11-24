using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using TurkSoft.Entities.GIBEntityDB;

namespace TurkSoft.Data.GibData
{
    /// <summary>
    /// GIB veritabanı DbContext'i.
    /// - CreatedAt / UpdatedAt alanlarını otomatik set eder.
    /// - Cascade delete (multiple path) hatalarına karşı tüm ilişkiler Restrict yapılmıştır.
    /// - UserAnnouncementRead ilişkisi özel olarak NoAction olarak ayarlanmıştır.
    /// - İlk migration sırasında Role ve User tabloları seed edilir.
    /// - Tüm tablolarda (UserId + İş Anahtarı) benzersiz index (IsActive=1 filtresiyle).
    /// - Soft delete: Delete çağrıları IsActive=false + DeleteDate olur.
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

            // Assembly'deki config sınıflarını uygula
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(GibAppDbContext).Assembly,
                x => x.Namespace != null && x.Namespace.Contains("TurkSoft.Data.GibData"));

            // === Global: Cascade Delete hatalarını engelle ===
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                fk.DeleteBehavior = DeleteBehavior.Restrict;

            // === Global: BaseEntity soft delete filtresi (IsActive = 1) + RowVersion ===
            ApplyGlobalSoftDeleteAndConcurrency(modelBuilder);

            // === Özel: UserAnnouncementRead -> User (NoAction) ===
            modelBuilder.Entity<UserAnnouncementRead>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserAnnouncementReads)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // === Mapping: UserRole ===
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // === Açık tekillikler (filtered unique) — kritik tablolar ===

            // Role: Name benzersiz (yalnız aktifler)
            modelBuilder.Entity<Role>(e =>
            {
                e.Property(x => x.Name).IsRequired().HasMaxLength(128);
                e.HasIndex(x => x.Name).IsUnique().HasFilter("[IsActive] = 1");
            });

            // User: Username + Email benzersiz (yalnız aktifler)
            modelBuilder.Entity<User>(e =>
            {
                e.Property(x => x.Username).IsRequired().HasMaxLength(256);
                e.Property(x => x.Email).IsRequired().HasMaxLength(256);

                e.HasIndex(x => x.Username).IsUnique().HasFilter("[IsActive] = 1");
                e.HasIndex(x => x.Email).IsUnique().HasFilter("[IsActive] = 1");
            });

            // UserRole: (UserId, RoleId) benzersiz (yalnız aktifler)
            modelBuilder.Entity<UserRole>(e =>
            {
                e.HasIndex(x => new { x.UserId, x.RoleId })
                 .IsUnique()
                 .HasFilter("[IsActive] = 1");
            });

            // Item: kullanıcı bazında ad tekilliği
            modelBuilder.Entity<Item>(e =>
            {
                if (e.Metadata.FindProperty("Name") != null)
                {
                    e.Property(x => x.Name).IsRequired().HasMaxLength(256);
                    e.HasIndex(x => new { x.UserId, x.Name })
                     .IsUnique()
                     .HasFilter("[IsActive] = 1");
                }
                else if (e.Metadata.FindProperty("Code") != null)
                {
                    e.Property("Code").IsRequired();
                    e.HasIndex("UserId", "Code")
                     .IsUnique()
                     .HasFilter("[IsActive] = 1");
                }
            });

            // Lookup örnekleri (varsa alanlarına göre)
            IfHasPropertyUniqueByUser(modelBuilder, typeof(Currency), "Code");
            IfHasPropertyUniqueByUser(modelBuilder, typeof(Country), "IsoCode", fallback: "Code");
            IfHasPropertyUniqueByUser(modelBuilder, typeof(Brand), "Name");
            IfHasPropertyUniqueByUser(modelBuilder, typeof(Category), "Name");
            IfHasPropertyUniqueByUser(modelBuilder, typeof(Unit), "ShortName", fallback: "Name");
            IfHasPropertyUniqueByUser(modelBuilder, typeof(Warehouse), "Name");

            // === Toplu otomatik tekillik (geri kalan tüm entity'ler için) ===
            modelBuilder.ApplyUserScopedUniqueness();

            // === SEED DATA ===
            SeedRoles(modelBuilder);
            SeedUsers(modelBuilder);
            SeedUserRoles(modelBuilder);
        }

        // ---- GLOBAL FILTER & ROWVERSION ----
        private static void ApplyGlobalSoftDeleteAndConcurrency(ModelBuilder b)
        {
            foreach (var et in b.Model.GetEntityTypes())
            {
                var clr = et.ClrType;
                if (clr == null || clr.IsAbstract) continue;

                // IsActive filtresi varsa uygula
                if (et.FindProperty("IsActive") != null)
                {
                    var param = Expression.Parameter(clr, "e");
                    var prop = Expression.Property(param, "IsActive");
                    var body = Expression.Equal(prop, Expression.Constant(true));
                    var lambda = Expression.Lambda(body, param);
                    et.SetQueryFilter(lambda);
                }

                // RowVersion
                if (et.FindProperty("RowVersion") != null)
                    b.Entity(clr).Property<byte[]>("RowVersion").IsRowVersion();

                // IsActive default true
                if (et.FindProperty("IsActive") != null)
                    b.Entity(clr).Property<bool>("IsActive").HasDefaultValue(true);
            }
        }

        // ---- HELPER: Eğer property varsa (UserId, <prop>) unique yap ----
        private static void IfHasPropertyUniqueByUser(ModelBuilder b, Type entity, string prop, string? fallback = null)
        {
            var et = b.Model.FindEntityType(entity);
            if (et == null) return;

            var hasUserId = et.FindProperty("UserId") != null;
            var hasProp = et.FindProperty(prop) != null;
            if (!hasUserId && fallback == null) return;

            if (!hasProp && fallback != null)
            {
                hasProp = et.FindProperty(fallback) != null;
                prop = fallback!;
            }

            if (hasUserId && hasProp)
            {
                b.Entity(entity).HasIndex(new[] { "UserId", prop })
                 .IsUnique()
                 .HasFilter("[IsActive] = 1");
            }
        }

        // ---- SEED: Roller ----
        private void SeedRoles(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2025, 11, 7, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", Desc = "Sistem yöneticisi", CreatedAt = seedDate, IsActive = true },
                new Role { Id = 2, Name = "Bayi", Desc = "Bayi kullanıcısı", CreatedAt = seedDate, IsActive = true },
                new Role { Id = 3, Name = "MaliMüşavir", Desc = "Mali müşavir kullanıcısı", CreatedAt = seedDate, IsActive = true },
                new Role { Id = 4, Name = "Firma", Desc = "Firma kullanıcısı", CreatedAt = seedDate, IsActive = true }
            );
        }

        // ---- SEED: Kullanıcılar (PBKDF2 hash) ----
        private void SeedUsers(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2025, 11, 7, 0, 0, 0, DateTimeKind.Utc);

            // Hash formatı: PBKDF2$<iter>$<salt>$<hash>
            const string adminHash = "PBKDF2$100000$dDSF2N132FQkI11U1m1m5A==$f5V1BBDJOOdE7QjoxPM+b557TmzNGPardO2QnHAho+I="; // Admin!123
            const string bayiHash = "PBKDF2$100000$QTFXdEp+oxfYXdv03gpFzg==$BGgBXx3qgMWCv0nh6/Web5cti+UztJ3EyfH0T12ZBF4="; // Bayi!123
            const string mmHash = "PBKDF2$100000$GQMf5cVH3D+Gk4hYHVeWRQ==$OjQXlOi7CCKny2cdt15McbKWGDuffOv8a8RSqS2CQs4="; // MM!123
            const string firmaHash = "PBKDF2$100000$nvzTYnu9jldsQTrX/0spEg==$3tnZ70MM9Fpzx0L8V+QyNLfq97hNSpppA+A7WaJXAMs="; // Firma!123

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Email = "admin@gib.com", PasswordHash = adminHash, CreatedAt = seedDate, IsActive = true },
                new User { Id = 2, Username = "bayi", Email = "bayi@gib.com", PasswordHash = bayiHash, CreatedAt = seedDate, IsActive = true },
                new User { Id = 3, Username = "malimusavir", Email = "mm@gib.com", PasswordHash = mmHash, CreatedAt = seedDate, IsActive = true },
                new User { Id = 4, Username = "firma", Email = "firma@gib.com", PasswordHash = firmaHash, CreatedAt = seedDate, IsActive = true }
            );
        }

        // ---- SEED: Kullanıcı-Rol eşlemesi ----
        private void SeedUserRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = 1, UserId = 1, RoleId = 1, IsActive = true }, // Admin
                new UserRole { Id = 2, UserId = 2, RoleId = 2, IsActive = true }, // Bayi
                new UserRole { Id = 3, UserId = 3, RoleId = 3, IsActive = true }, // MaliMüşavir
                new UserRole { Id = 4, UserId = 4, RoleId = 4, IsActive = true }  // Firma
            );
        }

        // ---- SaveChanges: audit + soft delete ----
        public override int SaveChanges()
        {
            ApplyAuditAndSoftDelete();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditAndSoftDelete();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Soft delete ve CreatedAt / UpdatedAt / DeleteDate alanlarını yönetir.
        /// DateTime ve DateTimeOffset tiplerini güvenli şekilde ele alır.
        /// </summary>
        private void ApplyAuditAndSoftDelete()
        {
            var utcNow = DateTimeOffset.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                // ---------- SOFT DELETE ----------
                if (entry.State == EntityState.Deleted &&
                    entry.Properties.Any(p => p.Metadata.Name == "IsActive"))
                {
                    entry.State = EntityState.Modified;
                    entry.Property("IsActive").CurrentValue = false;

                    var deleteProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "DeleteDate");
                    if (deleteProp != null)
                    {
                        var clrType = deleteProp.Metadata.ClrType;

                        if (IsDateTimeOffsetType(clrType))
                            deleteProp.CurrentValue = utcNow;
                        else if (IsDateTimeType(clrType))
                            deleteProp.CurrentValue = utcNow.UtcDateTime;
                    }

                    continue;
                }

                // ---------- ADDED (CreatedAt) ----------
                if (entry.State == EntityState.Added)
                {
                    var created = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                    if (created != null)
                    {
                        var clrType = created.Metadata.ClrType;

                        if (created.CurrentValue == null)
                        {
                            if (IsDateTimeOffsetType(clrType))
                                created.CurrentValue = utcNow;
                            else if (IsDateTimeType(clrType))
                                created.CurrentValue = utcNow.UtcDateTime;
                        }
                        else if (created.CurrentValue is DateTimeOffset dto && dto == default)
                        {
                            created.CurrentValue = utcNow;
                        }
                        else if (created.CurrentValue is DateTime dt && dt == default)
                        {
                            created.CurrentValue = utcNow.UtcDateTime;
                        }
                    }
                }
                // ---------- MODIFIED (UpdatedAt) ----------
                else if (entry.State == EntityState.Modified)
                {
                    var updated = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                    if (updated != null)
                    {
                        var clrType = updated.Metadata.ClrType;

                        if (IsDateTimeOffsetType(clrType))
                            updated.CurrentValue = utcNow;
                        else if (IsDateTimeType(clrType))
                            updated.CurrentValue = utcNow.UtcDateTime;
                    }
                }
            }
        }

        private static bool IsDateTimeOffsetType(Type t)
            => t == typeof(DateTimeOffset) || t == typeof(DateTimeOffset?);

        private static bool IsDateTimeType(Type t)
            => t == typeof(DateTime) || t == typeof(DateTime?);
    }

    // ==========================================================
    // TOPLU TEKİLLİK UYGULAYICI (UserId + İşAnahtarı, IsActive=1)
    // ==========================================================
    internal static class UserScopedUniquenessExtensions
    {
        // Genel adaylar
        private static readonly string[] GenericCandidates =
            { "Code", "Name", "Title", "TaxNo", "IsoCode", "Email", "Username", "Sku", "Barcode" };

        // Entity’ye özel tercih listeleri
        private static readonly System.Collections.Generic.Dictionary<string, string[]> PreferredByEntity = new()
        {
            { "Item",               new[] { "Code", "Name" } },
            { "CompanyInformation", new[] { "TaxNo" } },
            { "Currency",           new[] { "Code" } },
            { "Country",            new[] { "IsoCode", "Code" } },
            { "Brand",              new[] { "Name" } },
            { "Category",           new[] { "Name" } },
            { "Unit",               new[] { "ShortName", "Name" } },
            { "Warehouse",          new[] { "Name" } },
            { "UserRole",           new[] { "RoleId" } }, // (UserId, RoleId)
        };

        public static void ApplyUserScopedUniqueness(this ModelBuilder b)
        {
            const string Filter = "[IsActive] = 1";

            foreach (var et in b.Model.GetEntityTypes())
            {
                var clr = et.ClrType;
                if (clr == null || clr.IsAbstract) continue;

                // Yalnız UserId + IsActive olan tablolar
                if (et.FindProperty("UserId") is null || et.FindProperty("IsActive") is null) continue;

                // Önce tabloya özel bir iş anahtarı ara
                var keyProp = FindPreferredProperty(et);

                // Yoksa genel adaylardan birini seç
                keyProp ??= FindGenericProperty(et);

                if (keyProp == null)
                {
                    // İlişki tablosu gibi (UserId, RoleId) vb.
                    if (et.FindProperty("RoleId") != null)
                        CreateUnique(b, et, new[] { "UserId", "RoleId" }, Filter);
                    continue;
                }

                var pair = new[] { "UserId", keyProp.Name };

                if (!HasExactUniqueIndex(et, pair))
                {
                    CreateUnique(b, et, pair, Filter);

                    // String ise Required yap (NULL tekilliğini bozmasın)
                    if (keyProp.ClrType == typeof(string))
                        b.Entity(clr).Property(keyProp.Name).IsRequired();
                }
            }
        }

        private static IMutableProperty? FindPreferredProperty(IMutableEntityType et)
        {
            if (PreferredByEntity.TryGetValue(et.ClrType.Name, out var list))
            {
                foreach (var cand in list)
                {
                    var p = et.FindProperty(cand);
                    if (p != null) return p;
                }
            }
            return null;
        }

        private static IMutableProperty? FindGenericProperty(IMutableEntityType et)
        {
            foreach (var cand in GenericCandidates)
            {
                var p = et.FindProperty(cand);
                if (p != null) return p;
            }
            return null;
        }

        private static bool HasExactUniqueIndex(IMutableEntityType et, string[] propertyNames)
        {
            var target = propertyNames.ToArray();
            foreach (var ix in et.GetIndexes())
            {
                if (!ix.IsUnique) continue;
                var props = ix.Properties.Select(p => p.Name).ToArray();
                if (props.Length != target.Length) continue;
                if (props.Zip(target, (a, b) => a == b).All(x => x)) return true;
            }
            return false;
        }

        private static void CreateUnique(ModelBuilder b, IMutableEntityType et, string[] pair, string filter)
        {
            b.Entity(et.ClrType)
             .HasIndex(pair)
             .IsUnique()
             .HasFilter(filter);
        }
    }
}
