// TurkSoft.Data.Context/AppDbContext.cs

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Data.Configuration;

namespace TurkSoft.Data.Context
{
    /// <summary>
    /// - Fluent API config'leri otomatik yükler
    /// - Soft-delete global filtre uygular
    /// - CreateDate/UpdateDate/IsActive alanlarını güvenceye alır
    /// - Admin kullanıcısını seed eder
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ===== DbSet'ler =====
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Log> Loglar { get; set; }
        public DbSet<MailAyar> MailAyarlar { get; set; }
        public DbSet<MailGonderim> MailGonderimler { get; set; }
        public DbSet<SmsAyar> SmsAyarlar { get; set; }
        public DbSet<SmsGonderim> SmsGonderimler { get; set; }
        public DbSet<WhatsappAyar> WhatsappAyarlar { get; set; }
        public DbSet<WhatsappGonderim> WhatsappGonderimler { get; set; }

        public DbSet<Luca> Luca { get; set; }
        public DbSet<KeyAccount> KeyAccounts { get; set; }

        public DbSet<UrunTipi> UrunTipleri { get; set; }
        public DbSet<Paket> Paketler { get; set; }
        public DbSet<UrunFiyat> UrunFiyatlar { get; set; }
        public DbSet<PaketIskonto> PaketIskontolar { get; set; }

        public DbSet<Bayi> Bayiler { get; set; }
        public DbSet<BayiFirma> BayiFirmalari { get; set; }
        public DbSet<MaliMusavir> MaliMusavirler { get; set; }
        public DbSet<Firma> Firmalar { get; set; }

        public DbSet<Satis> Satislar { get; set; }
        public DbSet<SatisKalem> SatisKalemleri { get; set; }
        public DbSet<Lisans> Lisanslar { get; set; }
        public DbSet<LisansAdet> LisansAdetler { get; set; }

        public DbSet<Odeme> Odemeler { get; set; }
        public DbSet<SanalPos> SanalPoslar { get; set; }

        public DbSet<EntegrasyonHesabi> EntegrasyonHesaplari { get; set; }

        public DbSet<Lead> Leadler { get; set; }
        public DbSet<OpportunityAsama> OpportunityAsamalari { get; set; }
        public DbSet<Opportunity> Opportunities { get; set; }
        public DbSet<OpportunityAsamaGecis> OpportunityAsamaGecisleri { get; set; }
        public DbSet<Teklif> Teklifler { get; set; }
        public DbSet<TeklifKalem> TeklifKalemleri { get; set; }

        public DbSet<Aktivite> Aktiviteler { get; set; }
        public DbSet<AktiviteAtama> AktiviteAtamalari { get; set; }
        public DbSet<IletisimKisi> IletisimKisileri { get; set; }

        public DbSet<Fatura> Faturalar { get; set; }
        public DbSet<FaturaKalem> FaturaKalemleri { get; set; }

        public DbSet<BayiCari> BayiCariler { get; set; }
        public DbSet<BayiCariHareket> BayiCariHareketleri { get; set; }
        public DbSet<KomisyonOdemePlani> KomisyonOdemePlanlari { get; set; }
        public DbSet<BayiKomisyonTarife> BayiKomisyonTarifeleri { get; set; }

        public DbSet<FiyatListesi> FiyatListeleri { get; set; }
        public DbSet<FiyatListesiKalem> FiyatListesiKalemleri { get; set; }
        public DbSet<VergiOrani> VergiOranlari { get; set; }
        public DbSet<Kupon> Kuponlar { get; set; }

        public DbSet<Not> Notlar { get; set; }
        public DbSet<DosyaEk> DosyaEkleri { get; set; }
        public DbSet<Etiket> Etiketler { get; set; }
        public DbSet<EtiketIliski> EtiketIliskileri { get; set; }

        public DbSet<SistemBildirim> SistemBildirimleri { get; set; }
        public DbSet<OutboxMesaj> OutboxMesajlari { get; set; }
        public DbSet<WebhookAbonelik> WebhookAbonelikleri { get; set; }

        public DbSet<KullaniciBayi> KullaniciBayiler { get; set; } = default!;
        public DbSet<KullaniciFirma> KullaniciFirmalar { get; set; } = default!;
        public DbSet<KullaniciMaliMusavir> KullaniciMaliMusavirler { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Sadece AppDbContext'e ait config'leri uygula (EntityData namespace'i hariç)
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly,
                t => t.Namespace == "TurkSoft.Data.Configuration");

            // ---- Global soft-delete filtresi (DeleteDate == null olanlar) ----
            foreach (var et in modelBuilder.Model.GetEntityTypes())
            {
                if (et.IsOwned()) continue;

                var deleteDateProp = et.FindProperty("DeleteDate");
                if (deleteDateProp != null && deleteDateProp.ClrType == typeof(DateTime?))
                {
                    var parameter = Expression.Parameter(et.ClrType, "e");
                    var efPropertyMethod = typeof(EF).GetMethod(nameof(EF.Property))!
                        .MakeGenericMethod(typeof(DateTime?));
                    var deleteDateAccess = Expression.Call(efPropertyMethod, parameter, Expression.Constant("DeleteDate"));
                    var condition = Expression.Equal(deleteDateAccess, Expression.Constant(null, typeof(DateTime?)));
                    var lambda = Expression.Lambda(condition, parameter);
                    modelBuilder.Entity(et.ClrType).HasQueryFilter(lambda);
                }
            }

            var seed = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // ---- Seed: Kullanıcılar ----
            var adminId   = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var bayiUsrId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var mmUsrId   = Guid.Parse("33333333-3333-3333-3333-333333333333");

            modelBuilder.Entity<Kullanici>().HasData(
                new Kullanici { Id = adminId,   IsActive = true, CreateDate = seed, AdSoyad = "Sistem Yöneticisi", Eposta = "admin@turksoft.local",  Sifre = "Admin!12345",  Telefon = "+90 555 000 0000", Rol = "Admin" },
                new Kullanici { Id = bayiUsrId, IsActive = true, CreateDate = seed, AdSoyad = "Demo Bayi Kullanıcı", Eposta = "bayi@turksoft.local", Sifre = "Bayi!12345",  Telefon = "+90 555 000 0001", Rol = "Bayi" },
                new Kullanici { Id = mmUsrId,   IsActive = true, CreateDate = seed, AdSoyad = "Demo MM Kullanıcı",  Eposta = "mm@turksoft.local",    Sifre = "Mali!12345",  Telefon = "+90 555 000 0002", Rol = "MaliMusavir" }
            );

            // ---- Seed: Vergi Oranları ----
            modelBuilder.Entity<VergiOrani>().HasData(
                new VergiOrani { Id = Guid.Parse("AA000001-0000-0000-0000-000000000001"), IsActive = true, CreateDate = seed, Kod = "KDV0",  Oran = 0m  },
                new VergiOrani { Id = Guid.Parse("AA000001-0000-0000-0000-000000000002"), IsActive = true, CreateDate = seed, Kod = "KDV1",  Oran = 1m  },
                new VergiOrani { Id = Guid.Parse("AA000001-0000-0000-0000-000000000003"), IsActive = true, CreateDate = seed, Kod = "KDV10", Oran = 10m },
                new VergiOrani { Id = Guid.Parse("AA000001-0000-0000-0000-000000000004"), IsActive = true, CreateDate = seed, Kod = "KDV20", Oran = 20m }
            );

            // ---- Seed: Opportunity Aşamaları (CRM Pipeline) ----
            modelBuilder.Entity<OpportunityAsama>().HasData(
                new OpportunityAsama { Id = Guid.Parse("BB000001-0000-0000-0000-000000000001"), IsActive = true, CreateDate = seed, Kod = "NEW",   Ad = "Yeni Lead",       OlasilikYuzde = 10m  },
                new OpportunityAsama { Id = Guid.Parse("BB000001-0000-0000-0000-000000000002"), IsActive = true, CreateDate = seed, Kod = "QUAL",  Ad = "Nitelendirme",    OlasilikYuzde = 25m  },
                new OpportunityAsama { Id = Guid.Parse("BB000001-0000-0000-0000-000000000003"), IsActive = true, CreateDate = seed, Kod = "PROP",  Ad = "Teklif Verildi",  OlasilikYuzde = 50m  },
                new OpportunityAsama { Id = Guid.Parse("BB000001-0000-0000-0000-000000000004"), IsActive = true, CreateDate = seed, Kod = "NEG",   Ad = "Müzakere",        OlasilikYuzde = 75m  },
                new OpportunityAsama { Id = Guid.Parse("BB000001-0000-0000-0000-000000000005"), IsActive = true, CreateDate = seed, Kod = "WON",   Ad = "Kazanıldı",       OlasilikYuzde = 100m },
                new OpportunityAsama { Id = Guid.Parse("BB000001-0000-0000-0000-000000000006"), IsActive = true, CreateDate = seed, Kod = "LOST",  Ad = "Kaybedildi",      OlasilikYuzde = 0m   }
            );

            // ---- Seed: Ürün Tipleri ----
            var utYazilimId = Guid.Parse("CC000001-0000-0000-0000-000000000001");
            var utHizmetId  = Guid.Parse("CC000001-0000-0000-0000-000000000002");
            var utDestekId  = Guid.Parse("CC000001-0000-0000-0000-000000000003");

            modelBuilder.Entity<UrunTipi>().HasData(
                new UrunTipi { Id = utYazilimId, IsActive = true, CreateDate = seed, Ad = "Yazılım Lisansı", Aciklama = "ERP/Muhasebe yazılım lisansları" },
                new UrunTipi { Id = utHizmetId,  IsActive = true, CreateDate = seed, Ad = "Hizmet",          Aciklama = "Danışmanlık ve uygulama hizmetleri" },
                new UrunTipi { Id = utDestekId,  IsActive = true, CreateDate = seed, Ad = "Teknik Destek",   Aciklama = "Yıllık teknik destek paketleri" }
            );

            // ---- Seed: Paketler ----
            modelBuilder.Entity<Paket>().HasData(
                new Paket { Id = Guid.Parse("DD000001-0000-0000-0000-000000000001"), IsActive = true, CreateDate = seed, UrunTipiId = utYazilimId, Ad = "Starter",    Aciklama = "1 kullanıcı, temel muhasebe modülleri" },
                new Paket { Id = Guid.Parse("DD000001-0000-0000-0000-000000000002"), IsActive = true, CreateDate = seed, UrunTipiId = utYazilimId, Ad = "Professional", Aciklama = "5 kullanıcı, tüm ERP modülleri" },
                new Paket { Id = Guid.Parse("DD000001-0000-0000-0000-000000000003"), IsActive = true, CreateDate = seed, UrunTipiId = utYazilimId, Ad = "Enterprise",   Aciklama = "Sınırsız kullanıcı, özel entegrasyonlar" },
                new Paket { Id = Guid.Parse("DD000001-0000-0000-0000-000000000004"), IsActive = true, CreateDate = seed, UrunTipiId = utDestekId,  Ad = "Destek Temel", Aciklama = "İş günleri 09-18 telefon/e-posta destek" },
                new Paket { Id = Guid.Parse("DD000001-0000-0000-0000-000000000005"), IsActive = true, CreateDate = seed, UrunTipiId = utDestekId,  Ad = "Destek Premium", Aciklama = "7/24 öncelikli destek + yerinde servis" }
            );

            // ---- Seed: Bayi ----
            var demoBayiId = Guid.Parse("EE000001-0000-0000-0000-000000000001");
            modelBuilder.Entity<Bayi>().HasData(
                new Bayi { Id = demoBayiId, IsActive = true, CreateDate = seed, Kod = "DEMO-BAYI-001", Unvan = "Demo Bayi A.Ş.", Telefon = "+90 212 000 0001", Eposta = "bayi@demo.local", OlusturanKullaniciId = adminId }
            );

            // ---- Seed: KullaniciBayi (bayi kullanıcısını bayiye bağla) ----
            modelBuilder.Entity<KullaniciBayi>().HasData(
                new KullaniciBayi { Id = Guid.Parse("FF000001-0000-0000-0000-000000000001"), IsActive = true, CreateDate = seed, KullaniciId = bayiUsrId, BayiId = demoBayiId }
            );

            // ---- Seed: Mali Müşavir ----
            var demoMmId = Guid.Parse("A0000007-0000-0000-0000-000000000001");
            modelBuilder.Entity<MaliMusavir>().HasData(
                new MaliMusavir { Id = demoMmId, IsActive = true, CreateDate = seed, AdSoyad = "Demo Mali Müşavir", Telefon = "+90 212 000 0002", Eposta = "mm@demo.local", Unvan = "SMMM", VergiNo = "1234567890", TCKN = "12345678901", BayiId = demoBayiId }
            );

            // ---- Seed: KullaniciMaliMusavir ----
            modelBuilder.Entity<KullaniciMaliMusavir>().HasData(
                new KullaniciMaliMusavir { Id = Guid.Parse("A0000008-0000-0000-0000-000000000001"), IsActive = true, CreateDate = seed, KullaniciId = mmUsrId, MaliMusavirId = demoMmId }
            );

            // ---- Seed: Firma ----
            var demoFirmaId = Guid.Parse("A0000009-0000-0000-0000-000000000001");
            modelBuilder.Entity<Firma>().HasData(
                new Firma { Id = demoFirmaId, IsActive = true, CreateDate = seed, FirmaAdi = "Demo Şirket Ltd. Şti.", VergiNo = "9876543210", YetkiliAdSoyad = "Demo Yetkili", Telefon = "+90 212 000 0003", Eposta = "firma@demo.local", Adres = "Levent, İstanbul", MaliMusavirId = demoMmId, BayiId = demoBayiId }
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
                    var cd = entry.Property("CreateDate");
                    if (cd != null && (cd.CurrentValue == null || (DateTime)cd.CurrentValue == default))
                        cd.CurrentValue = utcNow;

                    var ia = entry.Property("IsActive");
                    if (ia != null && (ia.CurrentValue == null || (bool)ia.CurrentValue == default))
                        ia.CurrentValue = true;
                }
                else if (entry.State == EntityState.Modified)
                {
                    var ud = entry.Property("UpdateDate");
                    if (ud != null) ud.CurrentValue = utcNow;
                }

                // Soft-delete: kaydı silme, işaretle
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Property("DeleteDate").CurrentValue = utcNow;

                    var ia = entry.Property("IsActive");
                    if (ia != null) ia.CurrentValue = false;
                }
            }
        }
    }
}
