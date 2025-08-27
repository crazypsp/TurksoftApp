// GEREKLİ USING'LER
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Data.Context
{
    /// <summary>
    /// Uygulamanın temel DbContext'i.
    /// - Fluent API config'leri otomatik yükler
    /// - Soft-delete global filtre uygular
    /// - CreateDate/UpdateDate/IsActive alanlarını güvenceye alır
    /// - Admin kullanıcısını seed eder (sabit değerlerle)
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ===== Kullanıcı / Log / İletişim Ayarları =====
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Log> Loglar { get; set; }
        public DbSet<MailAyar> MailAyarlar { get; set; }
        public DbSet<MailGonderim> MailGonderimler { get; set; }
        public DbSet<SmsAyar> SmsAyarlar { get; set; }
        public DbSet<SmsGonderim> SmsGonderimler { get; set; }
        public DbSet<WhatsappAyar> WhatsappAyarlar { get; set; }
        public DbSet<WhatsappGonderim> WhatsappGonderimler { get; set; }

        // ===== Luca / KeyAccount =====
        public DbSet<Luca> Luca { get; set; }
        public DbSet<KeyAccount> KeyAccounts { get; set; }

        // ===== Ürün / Paket / Fiyat =====
        public DbSet<UrunTipi> UrunTipleri { get; set; }
        public DbSet<Paket> Paketler { get; set; }
        public DbSet<UrunFiyat> UrunFiyatlar { get; set; }
        public DbSet<PaketIskonto> PaketIskontolar { get; set; }

        // ===== Bayi / Firma / MM =====
        public DbSet<Bayi> Bayiler { get; set; }
        public DbSet<BayiFirma> BayiFirmalari { get; set; }
        public DbSet<MaliMusavir> MaliMusavirler { get; set; }
        public DbSet<Firma> Firmalar { get; set; }

        // ===== Satış / Kalem / Lisans =====
        public DbSet<Satis> Satislar { get; set; }
        public DbSet<SatisKalem> SatisKalemleri { get; set; }
        public DbSet<Lisans> Lisanslar { get; set; }
        public DbSet<LisansAdet> LisansAdetler { get; set; }

        // ===== Ödeme / SanalPos =====
        public DbSet<Odeme> Odemeler { get; set; }
        public DbSet<SanalPos> SanalPoslar { get; set; }

        // ===== Entegrasyon Hesapları =====
        public DbSet<EntegrasyonHesabi> EntegrasyonHesaplari { get; set; }

        // ===== CRM: Lead / Opportunity / Teklif =====
        public DbSet<Lead> Leadler { get; set; }
        public DbSet<OpportunityAsama> OpportunityAsamalari { get; set; }
        public DbSet<Opportunity> Opportunities { get; set; }
        public DbSet<OpportunityAsamaGecis> OpportunityAsamaGecisleri { get; set; }
        public DbSet<Teklif> Teklifler { get; set; }
        public DbSet<TeklifKalem> TeklifKalemleri { get; set; }

        // ===== Aktivite / İletişim =====
        public DbSet<Aktivite> Aktiviteler { get; set; }
        public DbSet<AktiviteAtama> AktiviteAtamalari { get; set; }
        public DbSet<IletisimKisi> IletisimKisileri { get; set; }

        // ===== Fatura =====
        public DbSet<Fatura> Faturalar { get; set; }
        public DbSet<FaturaKalem> FaturaKalemleri { get; set; }

        // ===== Bayi Cari & Komisyon =====
        public DbSet<BayiCari> BayiCariler { get; set; }
        public DbSet<BayiCariHareket> BayiCariHareketleri { get; set; }
        public DbSet<KomisyonOdemePlani> KomisyonOdemePlanlari { get; set; }
        public DbSet<BayiKomisyonTarife> BayiKomisyonTarifeleri { get; set; }

        // ===== Fiyat Listesi / Vergi / Kupon =====
        public DbSet<FiyatListesi> FiyatListeleri { get; set; }
        public DbSet<FiyatListesiKalem> FiyatListesiKalemleri { get; set; }
        public DbSet<VergiOrani> VergiOranlari { get; set; }
        public DbSet<Kupon> Kuponlar { get; set; }

        // ===== Not / DosyaEk / Etiket =====
        public DbSet<Not> Notlar { get; set; }
        public DbSet<DosyaEk> DosyaEkleri { get; set; }
        public DbSet<Etiket> Etiketler { get; set; }
        public DbSet<EtiketIliski> EtiketIliskileri { get; set; }

        // ===== Bildirim / Outbox / Webhook =====
        public DbSet<SistemBildirim> SistemBildirimleri { get; set; }
        public DbSet<OutboxMesaj> OutboxMesajlari { get; set; }
        public DbSet<WebhookAbonelik> WebhookAbonelikleri { get; set; }
        public DbSet<KullaniciBayi> KullaniciBayiler { get; set; } = default!;
        public DbSet<KullaniciFirma> KullaniciFirmalar { get; set; } = default!;
        public DbSet<KullaniciMaliMusavir> KullaniciMaliMusavirler { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tüm configuration sınıflarını otomatik uygula
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // ---- Global soft-delete filtresi (DeleteDate == null olanlar) ----
            foreach (var et in modelBuilder.Model.GetEntityTypes())
            {
                if (et.IsOwned()) continue;

                // "DeleteDate" var mı? (CLR'da olmasa bile shadow olarak var sayabiliriz)
                var deleteDateProp = et.FindProperty("DeleteDate");
                if (deleteDateProp != null && deleteDateProp.ClrType == typeof(DateTime?))
                {
                    // e => EF.Property<DateTime?>(e, "DeleteDate") == null
                    var parameter = Expression.Parameter(et.ClrType, "e");
                    var efPropertyMethod = typeof(EF).GetMethod(nameof(EF.Property))!
                        .MakeGenericMethod(typeof(DateTime?));
                    var deleteDateAccess = Expression.Call(efPropertyMethod, parameter, Expression.Constant("DeleteDate"));
                    var condition = Expression.Equal(deleteDateAccess, Expression.Constant(null, typeof(DateTime?)));
                    var lambda = Expression.Lambda(condition, parameter);
                    modelBuilder.Entity(et.ClrType).HasQueryFilter(lambda);
                }
            }

            // ---- Seed: Admin Kullanıcı (HasData sabit değer ister) ----
            var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var seedCreateDateUtc = new DateTime(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);

            modelBuilder.Entity<Kullanici>().HasData(new Kullanici
            {
                // BaseEntity alanları — initializer'lar HasData'da çalışmaz, sabit veriyoruz
                Id = adminId,
                IsActive = true,
                CreateDate = seedCreateDateUtc,
                UpdateDate = null,
                DeleteDate = null,

                // Kullanici alanları
                AdSoyad = "Sistem Yöneticisi",
                Eposta = "admin@turksoft.local",
                // Demo: düz metin. Canlıda HASH zorunlu.
                Sifre = "Admin!12345",
                Telefon = "+90 555 000 0000",
                Rol = "Admin",
                ProfilResmiUrl = null
            });
        }

        // Audit alanlarını güvenceye al
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
                    // BaseEntity initializer çalışsa da, güvenceye al:
                    var cd = entry.Property("CreateDate");
                    if (cd != null && (cd.CurrentValue == null || (DateTime)cd.CurrentValue == default))
                        cd.CurrentValue = utcNow;

                    var isActive = entry.Property("IsActive");
                    if (isActive != null && (bool)isActive.CurrentValue == default)
                        isActive.CurrentValue = true;
                }
                else if (entry.State == EntityState.Modified)
                {
                    var ud = entry.Property("UpdateDate");
                    if (ud != null) ud.CurrentValue = utcNow;
                }

                // Eğer hard delete yerine soft-delete yapmak istersek:
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Property("DeleteDate").CurrentValue = utcNow;
                }
            }
        }
    }
}
