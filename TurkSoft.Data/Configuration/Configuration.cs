// GEREKLİ USING'LER
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics;
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Data.Configuration
{
    public abstract class BaseEntityConfig<T> : IEntityTypeConfiguration<T> where T : class
    {
        public virtual void Configure(EntityTypeBuilder<T> b)
        {
            // 1) Türü belli gölge/gerçek property'ler
            b.Property<Guid>("Id");                 // CLR'da varsa onu bağlar, yoksa shadow olarak oluşturur
            b.Property<bool>("IsActive").HasDefaultValue(true);
            b.Property<DateTime>("CreateDate");
            b.Property<DateTime?>("UpdateDate");
            b.Property<DateTime?>("DeleteDate");

            // 2) Key ve concurrency
            b.HasKey("Id");

            b.Property<byte[]>("RowVersion")
             .IsRowVersion()
             .IsConcurrencyToken();

            // 3) Faydalı indeksler
            b.HasIndex("IsActive");
            b.HasIndex("CreateDate"); ;
        }
    }

    // ======== Owned helper ========
    public static class OwnedAdres
    {
        public static void Configure<T>(OwnedNavigationBuilder<T, Adres> o) where T : class
        {
            o.Property(p => p.Ulke).HasMaxLength(100);
            o.Property(p => p.Sehir).HasMaxLength(100);
            o.Property(p => p.Ilce).HasMaxLength(100);
            o.Property(p => p.PostaKodu).HasMaxLength(20);
            o.Property(p => p.AcikAdres).HasMaxLength(500);
        }
    }

    // ======== KULLANICI / LOG / AYARLAR ========
    public class KullaniciConfiguration : BaseEntityConfig<Kullanici>
    {
        public override void Configure(EntityTypeBuilder<Kullanici> b)
        {
            base.Configure(b);
            b.Property(x => x.AdSoyad).IsRequired().HasMaxLength(100);
            b.Property(x => x.Eposta).IsRequired().HasMaxLength(150);
            b.HasIndex(x => x.Eposta).IsUnique();
            b.Property(x => x.Sifre).IsRequired().HasMaxLength(100);
            b.Property(x => x.Telefon).HasMaxLength(20);
            b.Property(x => x.Rol).HasMaxLength(50);
            b.Property(x => x.ProfilResmiUrl).HasMaxLength(500);
        }
    }

    public class LogConfiguration : BaseEntityConfig<Log>
    {
        public override void Configure(EntityTypeBuilder<Log> b)
        {
            base.Configure(b);
            b.Property(x => x.Islem).IsRequired().HasMaxLength(255);
            b.Property(x => x.IpAdres).HasMaxLength(50);
            b.Property(x => x.Tarayici).HasMaxLength(100);
        }
    }

    public class MailAyarConfiguration : BaseEntityConfig<MailAyar>
    {
        public override void Configure(EntityTypeBuilder<MailAyar> b)
        {
            base.Configure(b);
            b.Property(x => x.Eposta).IsRequired().HasMaxLength(150);
            b.Property(x => x.SmtpServer).IsRequired().HasMaxLength(100);
            b.Property(x => x.Sifre).IsRequired().HasMaxLength(100);
            // Ek alanlar
            b.Property(x => x.Port).HasDefaultValue(587);
            b.Property(x => x.SSLKullan).HasDefaultValue(true);

            // (Opsiyonel) aynı e-posta & sunucu kombinasyonu birden çok kayda izinli olsun
            b.HasIndex(x => new { x.Eposta, x.SmtpServer });
        }
    }

    public class MailGonderimConfiguration : BaseEntityConfig<MailGonderim>
    {
        public override void Configure(EntityTypeBuilder<MailGonderim> b)
        {
            base.Configure(b);
            b.Property(x => x.Alici).IsRequired().HasMaxLength(150);
            b.Property(x => x.Konu).HasMaxLength(250);
        }
    }

    public class SmsAyarConfiguration : BaseEntityConfig<SmsAyar>
    {
        public override void Configure(EntityTypeBuilder<SmsAyar> b)
        {
            base.Configure(b);
            b.Property(x => x.ApiKey).IsRequired().HasMaxLength(150);
            b.Property(x => x.ApiSecret).HasMaxLength(150);
            b.Property(x => x.GondericiAdi).HasMaxLength(100);
        }
    }

    public class SmsGonderimConfiguration : BaseEntityConfig<SmsGonderim>
    {
        public override void Configure(EntityTypeBuilder<SmsGonderim> b)
        {
            base.Configure(b);
            b.Property(x => x.AliciNumara).IsRequired().HasMaxLength(20);
            b.Property(x => x.Mesaj).HasMaxLength(1000);
        }
    }

    public class WhatsappAyarConfiguration : BaseEntityConfig<WhatsappAyar>
    {
        public override void Configure(EntityTypeBuilder<WhatsappAyar> b)
        {
            base.Configure(b);
            b.Property(x => x.ApiUrl).IsRequired().HasMaxLength(200);
            b.Property(x => x.Token).HasMaxLength(500);
            b.Property(x => x.Numara).HasMaxLength(20);
        }
    }

    public class WhatsappGonderimConfiguration : BaseEntityConfig<WhatsappGonderim>
    {
        public override void Configure(EntityTypeBuilder<WhatsappGonderim> b)
        {
            base.Configure(b);
            b.Property(x => x.AliciNumara).IsRequired().HasMaxLength(20);
            b.Property(x => x.Mesaj).HasMaxLength(1000);
        }
    }

    // ======== LUCA / KEYACCOUNT ========
    public class LucaConfiguration : BaseEntityConfig<Luca>
    {
        public override void Configure(EntityTypeBuilder<Luca> b)
        {
            base.Configure(b);
            b.ToTable("Luca");
            b.Property(x => x.UyeNo).IsRequired().HasMaxLength(50);
            b.Property(x => x.KullaniciAdi).IsRequired().HasMaxLength(100);
            b.Property(x => x.Parola).IsRequired().HasMaxLength(200);

            b.HasMany(x => x.MaliMusavirs)
             .WithMany()
             .UsingEntity<Dictionary<string, object>>(
                "Luca_MaliMusavir",
                r => r.HasOne<MaliMusavir>().WithMany().HasForeignKey("MaliMusavirId").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<Luca>().WithMany().HasForeignKey("LucaId").OnDelete(DeleteBehavior.Cascade),
                j => { j.ToTable("Luca_MaliMusavir"); j.HasKey("LucaId", "MaliMusavirId"); });
        }
    }

    public class KeyAccountConfiguration : BaseEntityConfig<KeyAccount>
    {
        public override void Configure(EntityTypeBuilder<KeyAccount> b)
        {
            base.Configure(b);
            b.ToTable("KeyAccount");
            b.Property(x => x.Kod).IsRequired().HasMaxLength(50);
            b.Property(x => x.Aciklama).HasMaxLength(300);
            b.HasIndex(x => x.Kod).IsUnique();

            b.HasMany(x => x.MaliMusavirs)
             .WithMany()
             .UsingEntity<Dictionary<string, object>>(
                "KeyAccount_MaliMusavir",
                r => r.HasOne<MaliMusavir>().WithMany().HasForeignKey("MaliMusavirId").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<KeyAccount>().WithMany().HasForeignKey("KeyAccountId").OnDelete(DeleteBehavior.Cascade),
                j => { j.ToTable("KeyAccount_MaliMusavir"); j.HasKey("KeyAccountId", "MaliMusavirId"); });
        }
    }

    // ======== ÜRÜN / PAKET / FİYAT ========
    public class UrunTipiConfiguration : BaseEntityConfig<UrunTipi>
    {
        public override void Configure(EntityTypeBuilder<UrunTipi> b)
        {
            base.Configure(b);
            b.Property(x => x.Ad).IsRequired().HasMaxLength(100);
            b.Property(x => x.Aciklama).HasMaxLength(300);
        }
    }

    public class PaketConfiguration : BaseEntityConfig<Paket>
    {
        public override void Configure(EntityTypeBuilder<Paket> b)
        {
            base.Configure(b);
            b.Property(x => x.Ad).IsRequired().HasMaxLength(100);
            b.Property(x => x.Aciklama).HasMaxLength(300);

            b.HasOne(x => x.UrunTipi)
             .WithMany(u => u.Paketler)
             .HasForeignKey(x => x.UrunTipiId)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class UrunFiyatConfiguration : BaseEntityConfig<UrunFiyat>
    {
        public override void Configure(EntityTypeBuilder<UrunFiyat> b)
        {
            base.Configure(b);
            b.Property(x => x.Fiyat).HasColumnType("decimal(18,2)");
            b.Property(x => x.ParaBirimi).HasMaxLength(10);

            b.HasOne(x => x.Paket)
             .WithMany(p => p.UrunFiyatlar)
             .HasForeignKey(x => x.PaketId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.PaketId, x.GecerlilikBaslangic, x.GecerlilikBitis });
        }
    }

    public class PaketIskontoConfiguration : BaseEntityConfig<PaketIskonto>
    {
        public override void Configure(EntityTypeBuilder<PaketIskonto> b)
        {
            base.Configure(b);
            b.ToTable("PaketIskonto");
            b.Property(x => x.IskontoYuzde).HasColumnType("decimal(5,2)");

            b.HasOne(x => x.Paket)
             .WithMany(p => p.PaketIskontolar)
             .HasForeignKey(x => x.PaketId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Bayi)
             .WithMany(p => p.PaketIskontolari)
             .HasForeignKey(x => x.BayiId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.PaketId, x.BayiId, x.Baslangic, x.Bitis });
        }
    }

    // ======== BAYİ / FİRMA / MALİ MÜŞAVİR ========
    public class BayiConfiguration : BaseEntityConfig<Bayi>
    {
        public override void Configure(EntityTypeBuilder<Bayi> b)
        {
            base.Configure(b);
            b.ToTable("Bayi");
            b.Property(x => x.Kod).IsRequired().HasMaxLength(30);
            b.HasIndex(x => x.Kod).IsUnique();
            b.Property(x => x.Unvan).IsRequired().HasMaxLength(200);
            b.Property(x => x.Telefon).HasMaxLength(20);
            b.Property(x => x.Eposta).HasMaxLength(150);

            b.HasOne(x => x.OlusturanKullanici)
             .WithMany()
             .HasForeignKey(x => x.OlusturanKullaniciId)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class BayiFirmaConfiguration : BaseEntityConfig<BayiFirma>
    {
        public override void Configure(EntityTypeBuilder<BayiFirma> b)
        {
            base.Configure(b);
            b.ToTable("BayiFirma");
            b.Property(x => x.VergiNo).HasMaxLength(15);
            b.Property(x => x.Iban).HasMaxLength(34);
            b.Property(x => x.Adres).HasMaxLength(500);

            b.HasOne(x => x.Bayi)
             .WithOne(x => x.BayiFirma)
             .HasForeignKey<BayiFirma>(x => x.BayiId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class MaliMusavirConfiguration : BaseEntityConfig<MaliMusavir>
    {
        public override void Configure(EntityTypeBuilder<MaliMusavir> b)
        {
            base.Configure(b);
            b.Property(x => x.AdSoyad).IsRequired().HasMaxLength(100);
            b.Property(x => x.Telefon).HasMaxLength(20);
            b.Property(x => x.Eposta).HasMaxLength(150);
            b.Property(x => x.Unvan).HasMaxLength(100);
            b.Property(x => x.VergiNo).HasMaxLength(15);
            b.Property(x => x.TCKN).HasMaxLength(11);

            b.HasOne(x => x.Bayi)
             .WithMany(p => p.MaliMusavirler)
             .HasForeignKey(x => x.BayiId)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class FirmaConfiguration : BaseEntityConfig<Firma>
    {
        public override void Configure(EntityTypeBuilder<Firma> b)
        {
            base.Configure(b);
            b.Property(x => x.FirmaAdi).IsRequired().HasMaxLength(150);
            b.Property(x => x.VergiNo).HasMaxLength(15);
            b.Property(x => x.YetkiliAdSoyad).HasMaxLength(100);
            b.Property(x => x.Telefon).HasMaxLength(20);
            b.Property(x => x.Eposta).HasMaxLength(150);
            b.Property(x => x.Adres).HasMaxLength(500);

            b.HasOne(x => x.MaliMusavir)
             .WithMany(m => m.Firmalar)
             .HasForeignKey(x => x.MaliMusavirId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Bayi)
             .WithMany(m => m.Firmalar)
             .HasForeignKey(x => x.BayiId)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }

    // ======== SATIŞ / KALEM / LİSANS ========
    public class SatisConfiguration : BaseEntityConfig<Satis>
    {
        public override void Configure(EntityTypeBuilder<Satis> b)
        {
            base.Configure(b);
            b.ToTable("Satis");
            b.Property(x => x.SatisNo).IsRequired().HasMaxLength(30);
            b.HasIndex(x => x.SatisNo).IsUnique();
            b.Property(x => x.ToplamTutar).HasColumnType("decimal(18,2)");
            b.Property(x => x.NetTutar).HasColumnType("decimal(18,2)");
            b.Property(x => x.IskontoTutar).HasColumnType("decimal(18,2)");
            b.Property(x => x.KDVOrani).HasColumnType("decimal(5,2)");
            b.Property(x => x.KDVTutar).HasColumnType("decimal(18,2)");
            b.Property(x => x.SatisDurumu).HasConversion<int>();
            b.HasIndex(x => new { x.BayiId, x.MaliMusavirId, x.SatisTarihi });

            b.HasOne(x => x.Bayi).WithMany(p => p.Satislar).HasForeignKey(x => x.BayiId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.MaliMusavir).WithMany(p => p.Satislar).HasForeignKey(x => x.MaliMusavirId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Firma).WithMany(p => p.Satislar).HasForeignKey(x => x.FirmaId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Paket).WithMany(p => p.Satislar).HasForeignKey(x => x.PaketId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class SatisKalemConfiguration : BaseEntityConfig<SatisKalem>
    {
        public override void Configure(EntityTypeBuilder<SatisKalem> b)
        {
            base.Configure(b);
            b.ToTable("SatisKalem");
            b.Property(x => x.Miktar).HasDefaultValue(1);
            b.Property(x => x.BirimFiyat).HasColumnType("decimal(18,2)");
            b.Property(x => x.Tutar).HasColumnType("decimal(18,2)");

            b.HasOne(x => x.Satis).WithMany(p => p.Kalemler).HasForeignKey(x => x.SatisId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Paket).WithMany().HasForeignKey(x => x.PaketId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class LisansConfiguration : BaseEntityConfig<Lisans>
    {
        public override void Configure(EntityTypeBuilder<Lisans> b)
        {
            base.Configure(b);
            b.Property(x => x.LisansAnahtari).IsRequired().HasMaxLength(100);
            b.HasOne(x => x.Satis).WithMany(p => p.Lisanslar).HasForeignKey(x => x.SatisId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.BaslangicTarihi, x.BitisTarihi });
        }
    }

    public class LisansAdetConfiguration : BaseEntityConfig<LisansAdet>
    {
        public override void Configure(EntityTypeBuilder<LisansAdet> b)
        {
            base.Configure(b);
        }
    }

    // ======== ÖDEME / POS ========
    public class OdemeConfiguration : BaseEntityConfig<Odeme>
    {
        public override void Configure(EntityTypeBuilder<Odeme> b)
        {
            base.Configure(b);
            b.ToTable("Odeme");
            b.Property(x => x.Tutar).HasColumnType("decimal(18,2)");
            b.Property(x => x.KomisyonOrani).HasColumnType("decimal(5,2)");
            b.Property(x => x.KomisyonTutar).HasColumnType("decimal(18,2)");
            b.Property(x => x.NetTutar).HasColumnType("decimal(18,2)");
            b.Property(x => x.OdemeYontemi).HasConversion<int>();
            b.Property(x => x.OdemeDurumu).HasConversion<int>();
            b.HasOne(x => x.Satis).WithMany(p => p.Odemeler).HasForeignKey(x => x.SatisId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.SanalPos).WithMany(p => p.Odemeler).HasForeignKey(x => x.SanalPosId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.SatisId, x.OdemeTarihi, x.OdemeDurumu });
            b.HasIndex(x => x.SaglayiciIslemNo);
        }
    }

    public class SanalPosConfiguration : BaseEntityConfig<SanalPos>
    {
        public override void Configure(EntityTypeBuilder<SanalPos> b)
        {
            base.Configure(b);
            b.ToTable("SanalPos");
            b.Property(x => x.Saglayici).IsRequired().HasMaxLength(50);
            b.Property(x => x.ApiKey).HasMaxLength(300);
            b.Property(x => x.ApiSecret).HasMaxLength(300);
            b.Property(x => x.MerchantId).HasMaxLength(100);
            b.Property(x => x.PosAnahtar).HasMaxLength(300);
            b.Property(x => x.BaseApiUrl).HasMaxLength(300);
            b.Property(x => x.StandartKomisyonYuzde).HasColumnType("decimal(5,2)");
            b.HasOne(x => x.Bayi).WithMany(p => p.SanalPoslar).HasForeignKey(x => x.BayiId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.BayiId, x.Saglayici });
        }
    }

    // ======== ENTEGRASYON ========
    public class EntegrasyonHesabiConfiguration : BaseEntityConfig<EntegrasyonHesabi>
    {
        public override void Configure(EntityTypeBuilder<EntegrasyonHesabi> b)
        {
            base.Configure(b);

            b.ToTable("EntegrasyonHesabi");

            b.Property(x => x.SistemTipi).HasConversion<int>();
            b.Property(x => x.Host).HasMaxLength(200);
            b.Property(x => x.VeritabaniAdi).HasMaxLength(150);
            b.Property(x => x.KullaniciAdi).HasMaxLength(100);
            b.Property(x => x.Parola).HasMaxLength(300);
            b.Property(x => x.ApiUrl).HasMaxLength(300);
            b.Property(x => x.ApiKey).HasMaxLength(300);
            b.Property(x => x.Aciklama).HasMaxLength(500);

            b.HasOne(x => x.MaliMusavir)
             .WithMany(p => p.EntegrasyonHesaplari)
             .HasForeignKey(x => x.MaliMusavirId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Firma)
             .WithMany(p => p.EntegrasyonHesaplari)
             .HasForeignKey(x => x.FirmaId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.MaliMusavirId, x.FirmaId, x.SistemTipi });
        }
    }

    // ======== PIPELINE ========
    public class LeadConfiguration : BaseEntityConfig<Lead>
    {
        public override void Configure(EntityTypeBuilder<Lead> b)
        {
            base.Configure(b);
            b.Property(x => x.LeadNo).IsRequired().HasMaxLength(30);
            b.HasIndex(x => x.LeadNo).IsUnique();
            b.Property(x => x.Kaynak).HasMaxLength(50);
            b.Property(x => x.Unvan).HasMaxLength(200);
            b.OwnsOne(x => x.Adres, o => OwnedAdres.Configure(o));
            b.HasOne(x => x.Bayi).WithMany(p => p.Leadler).HasForeignKey(x => x.BayiId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.SorumluKullanici).WithMany().HasForeignKey(x => x.SorumluKullaniciId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class OpportunityAsamaConfiguration : BaseEntityConfig<OpportunityAsama>
    {
        public override void Configure(EntityTypeBuilder<OpportunityAsama> b)
        {
            base.Configure(b);
            b.Property(x => x.Kod).IsRequired().HasMaxLength(30);
            b.Property(x => x.Ad).IsRequired().HasMaxLength(100);
            b.Property(x => x.OlasilikYuzde).HasColumnType("decimal(5,2)");
            b.HasIndex(x => x.Kod).IsUnique();
        }
    }

    public class OpportunityConfiguration : BaseEntityConfig<Opportunity>
    {
        public override void Configure(EntityTypeBuilder<Opportunity> b)
        {
            base.Configure(b);
            b.Property(x => x.FirsatNo).IsRequired().HasMaxLength(30);
            b.HasIndex(x => x.FirsatNo).IsUnique();
            b.Property(x => x.TahminiTutar).HasColumnType("decimal(18,2)");
            b.Property(x => x.Durum).HasConversion<int>();
            b.HasOne(x => x.Bayi).WithMany(p => p.Firsatlar).HasForeignKey(x => x.BayiId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.MaliMusavir).WithMany(p => p.Firsatlar).HasForeignKey(x => x.MaliMusavirId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Firma).WithMany(p => p.Firsatlar).HasForeignKey(x => x.FirmaId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Asama).WithMany().HasForeignKey(x => x.AsamaId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.BayiId, x.AsamaId, x.OlusturmaTarihi });
        }
    }

    public class OpportunityAsamaGecisConfiguration : BaseEntityConfig<OpportunityAsamaGecis>
    {
        public override void Configure(EntityTypeBuilder<OpportunityAsamaGecis> b)
        {
            base.Configure(b);
            b.HasOne(x => x.Opportunity).WithMany(p => p.AsamaGecisleri).HasForeignKey(x => x.OpportunityId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.FromAsama).WithMany().HasForeignKey(x => x.FromAsamaId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.ToAsama).WithMany().HasForeignKey(x => x.ToAsamaId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.GecisTarihi).IsRequired();
        }
    }

    public class TeklifConfiguration : BaseEntityConfig<Teklif>
    {
        public override void Configure(EntityTypeBuilder<Teklif> b)
        {
            base.Configure(b);
            b.Property(x => x.TeklifNo).IsRequired().HasMaxLength(30);
            b.HasIndex(x => x.TeklifNo).IsUnique();
            b.HasOne(x => x.Opportunity).WithMany(p => p.Teklifler).HasForeignKey(x => x.OpportunityId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Paket).WithMany(p => p.Teklifler).HasForeignKey(x => x.PaketId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Toplam).HasColumnType("decimal(18,2)");
            b.Property(x => x.Net).HasColumnType("decimal(18,2)");
            b.Property(x => x.Kdvtutar).HasColumnType("decimal(18,2)");
            b.Property(x => x.Kdvoran).HasColumnType("decimal(5,2)");
            b.Property(x => x.Durum).HasConversion<int>();
        }
    }

    public class TeklifKalemConfiguration : BaseEntityConfig<TeklifKalem>
    {
        public override void Configure(EntityTypeBuilder<TeklifKalem> b)
        {
            base.Configure(b);
            b.HasOne(x => x.Teklif).WithMany(p => p.Kalemler).HasForeignKey(x => x.TeklifId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Paket).WithMany().HasForeignKey(x => x.PaketId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Miktar).HasDefaultValue(1);
            b.Property(x => x.BirimFiyat).HasColumnType("decimal(18,2)");
            b.Property(x => x.Tutar).HasColumnType("decimal(18,2)");
        }
    }

    // ======== AKTİVİTE / İLETİŞİM ========
    public class AktiviteConfiguration : BaseEntityConfig<Aktivite>
    {
        public override void Configure(EntityTypeBuilder<Aktivite> b)
        {
            base.Configure(b);
            b.Property(x => x.Konu).IsRequired().HasMaxLength(200);
            b.Property(x => x.Tur).HasConversion<int>();
            b.Property(x => x.Durum).HasConversion<int>();
            b.HasOne(x => x.Bayi).WithMany(p => p.Aktiviteler).HasForeignKey(x => x.BayiId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.IlgiliKullanici).WithMany().HasForeignKey(x => x.IlgiliKullaniciId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.BayiId, x.Tur, x.PlanlananTarih });
        }
    }

    public class AktiviteAtamaConfiguration : BaseEntityConfig<AktiviteAtama>
    {
        public override void Configure(EntityTypeBuilder<AktiviteAtama> b)
        {
            base.Configure(b);
            b.HasOne(x => x.Aktivite).WithMany(x => x.Atamalar).HasForeignKey(x => x.AktiviteId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Kullanici).WithMany().HasForeignKey(x => x.KullaniciId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Rol).HasMaxLength(50);
            b.HasIndex(x => new { x.AktiviteId, x.KullaniciId }).IsUnique();
        }
    }

    public class IletisimKisiConfiguration : BaseEntityConfig<IletisimKisi>
    {
        public override void Configure(EntityTypeBuilder<IletisimKisi> b)
        {
            base.Configure(b);
            b.Property(x => x.AdSoyad).IsRequired().HasMaxLength(150);
            b.Property(x => x.Eposta).HasMaxLength(150);
            b.Property(x => x.Telefon).HasMaxLength(20);
            b.OwnsOne(x => x.Adres, o => OwnedAdres.Configure(o));
            b.HasOne(x => x.Firma).WithMany(p => p.IletisimKisileri).HasForeignKey(x => x.FirmaId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.FirmaId, x.AdSoyad });
        }
    }

    // ======== FATURA ========
    public class FaturaConfiguration : BaseEntityConfig<Fatura>
    {
        public override void Configure(EntityTypeBuilder<Fatura> b)
        {
            base.Configure(b);
            b.Property(x => x.FaturaNo).IsRequired().HasMaxLength(30);
            b.HasIndex(x => x.FaturaNo).IsUnique();
            b.Property(x => x.Tip).HasConversion<int>();
            b.Property(x => x.Durum).HasConversion<int>();
            b.HasOne(x => x.Satis).WithMany(p => p.Faturalar).HasForeignKey(x => x.SatisId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Firma).WithMany(p => p.Faturalar).HasForeignKey(x => x.FirmaId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Bayi).WithMany(p => p.Faturalar).HasForeignKey(x => x.BayiId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Toplam).HasColumnType("decimal(18,2)");
            b.Property(x => x.Net).HasColumnType("decimal(18,2)");
            b.Property(x => x.Kdvtutar).HasColumnType("decimal(18,2)");
            b.Property(x => x.Kdvoran).HasColumnType("decimal(5,2)");
        }
    }

    public class FaturaKalemConfiguration : BaseEntityConfig<FaturaKalem>
    {
        public override void Configure(EntityTypeBuilder<FaturaKalem> b)
        {
            base.Configure(b);
            b.HasOne(x => x.Fatura).WithMany(p => p.Kalemler).HasForeignKey(x => x.FaturaId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Paket).WithMany().HasForeignKey(x => x.PaketId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Miktar).HasDefaultValue(1);
            b.Property(x => x.BirimFiyat).HasColumnType("decimal(18,2)");
            b.Property(x => x.Tutar).HasColumnType("decimal(18,2)");
        }
    }

    // ======== BAYİ CARİ & KOMİSYON ========
    public class BayiCariConfiguration : BaseEntityConfig<BayiCari>
    {
        public override void Configure(EntityTypeBuilder<BayiCari> b)
        {
            base.Configure(b);
            b.HasOne(x => x.Bayi).WithMany(p => p.BayiCariler).HasForeignKey(x => x.BayiId).OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.Bakiye).HasColumnType("decimal(18,2)");
            b.HasIndex(x => x.BayiId).IsUnique();
        }
    }

    public class BayiCariHareketConfiguration : BaseEntityConfig<BayiCariHareket>
    {
        public override void Configure(EntityTypeBuilder<BayiCariHareket> b)
        {
            base.Configure(b);
            b.HasOne(x => x.BayiCari).WithMany(p => p.Hareketler).HasForeignKey(x => x.BayiCariId).OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.Tip).HasConversion<int>();
            b.Property(x => x.Tutar).HasColumnType("decimal(18,2)");
            b.HasIndex(x => new { x.BayiCariId, x.IslemTarihi });
        }
    }

    public class KomisyonOdemePlaniConfiguration : BaseEntityConfig<KomisyonOdemePlani>
    {
        public override void Configure(EntityTypeBuilder<KomisyonOdemePlani> b)
        {
            base.Configure(b);
            b.HasOne(x => x.Bayi).WithMany(p => p.KomisyonOdemePlanlari).HasForeignKey(x => x.BayiId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.BayiId, x.DonemYil, x.DonemAy }).IsUnique();
            b.Property(x => x.ToplamKomisyon).HasColumnType("decimal(18,2)");
            b.Property(x => x.Durum).HasConversion<int>();
        }
    }

    public class BayiKomisyonTarifeConfiguration : BaseEntityConfig<BayiKomisyonTarife>
    {
        public override void Configure(EntityTypeBuilder<BayiKomisyonTarife> b)
        {
            base.Configure(b);
            b.ToTable("BayiKomisyonTarife");
            b.Property(x => x.KomisyonYuzde).HasColumnType("decimal(5,2)");
            b.HasOne(x => x.Bayi).WithMany(p => p.KomisyonTarifeleri).HasForeignKey(x => x.BayiId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Paket).WithMany(p => p.BayiKomisyonTarifeleri).HasForeignKey(x => x.PaketId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.BayiId, x.PaketId, x.Baslangic, x.Bitis });
        }
    }

    // ======== FİYAT LİSTESİ / VERGİ / KUPON ========
    public class FiyatListesiConfiguration : BaseEntityConfig<FiyatListesi>
    {
        public override void Configure(EntityTypeBuilder<FiyatListesi> b)
        {
            base.Configure(b);
            b.Property(x => x.Kod).IsRequired().HasMaxLength(30);
            b.HasIndex(x => x.Kod).IsUnique();
            b.Property(x => x.Ad).IsRequired().HasMaxLength(100);
            b.HasOne(x => x.Bayi).WithMany(p => p.FiyatListeleri).HasForeignKey(x => x.BayiId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.BayiId, x.Baslangic, x.Bitis });
        }
    }

    public class FiyatListesiKalemConfiguration : BaseEntityConfig<FiyatListesiKalem>
    {
        public override void Configure(EntityTypeBuilder<FiyatListesiKalem> b)
        {
            base.Configure(b);
            b.HasOne(x => x.FiyatListesi).WithMany(p => p.Kalemler).HasForeignKey(x => x.FiyatListesiId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Paket).WithMany().HasForeignKey(x => x.PaketId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.BirimFiyat).HasColumnType("decimal(18,2)");
            b.HasIndex(x => new { x.FiyatListesiId, x.PaketId }).IsUnique();
        }
    }

    public class VergiOraniConfiguration : BaseEntityConfig<VergiOrani>
    {
        public override void Configure(EntityTypeBuilder<VergiOrani> b)
        {
            base.Configure(b);
            b.Property(x => x.Kod).IsRequired().HasMaxLength(20);
            b.Property(x => x.Oran).HasColumnType("decimal(5,2)");
            b.HasIndex(x => x.Kod).IsUnique();
        }
    }

    public class KuponConfiguration : BaseEntityConfig<Kupon>
    {
        public override void Configure(EntityTypeBuilder<Kupon> b)
        {
            base.Configure(b);
            b.Property(x => x.Kod).IsRequired().HasMaxLength(30);
            b.HasIndex(x => x.Kod).IsUnique();
            b.Property(x => x.IndirimYuzde).HasColumnType("decimal(5,2)");
            b.Property(x => x.MaksKullanim).HasDefaultValue(1);
            b.HasOne(x => x.Bayi).WithMany(p => p.Kuponlar).HasForeignKey(x => x.BayiId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    // ======== NOT / EK / ETİKET ========
    public class NotConfiguration : BaseEntityConfig<Not>
    {
        public override void Configure(EntityTypeBuilder<Not> b)
        {
            base.Configure(b);
            b.Property(x => x.Baslik).HasMaxLength(150);
            b.Property(x => x.Icerik).HasMaxLength(4000);
            b.Property(x => x.Tip).HasConversion<int>();
            b.HasOne(x => x.Olusturan).WithMany().HasForeignKey(x => x.OlusturanUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.IlgiliId, x.IlgiliTip });
        }
    }

    public class DosyaEkConfiguration : BaseEntityConfig<DosyaEk>
    {
        public override void Configure(EntityTypeBuilder<DosyaEk> b)
        {
            base.Configure(b);
            b.Property(x => x.DosyaAdi).HasMaxLength(255);
            b.Property(x => x.IcerikTipi).HasMaxLength(100);
            b.Property(x => x.Yol).HasMaxLength(500);
            b.HasIndex(x => new { x.IlgiliId, x.IlgiliTip });
        }
    }

    public class EtiketConfiguration : BaseEntityConfig<Etiket>
    {
        public override void Configure(EntityTypeBuilder<Etiket> b)
        {
            base.Configure(b);
            b.Property(x => x.Ad).IsRequired().HasMaxLength(50);
            b.HasIndex(x => x.Ad);
        }
    }

    public class EtiketIliskiConfiguration : BaseEntityConfig<EtiketIliski>
    {
        public override void Configure(EntityTypeBuilder<EtiketIliski> b)
        {
            base.Configure(b);
            b.HasOne(x => x.Etiket).WithMany(p => p.Iliskiler).HasForeignKey(x => x.EtiketId).OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.IlgiliTip).HasConversion<int>();
            b.HasIndex(x => new { x.EtiketId, x.IlgiliId, x.IlgiliTip }).IsUnique();
        }
    }

    // ======== BİLDİRİM / OUTBOX / WEBHOOK ========
    public class SistemBildirimConfiguration : BaseEntityConfig<SistemBildirim>
    {
        public override void Configure(EntityTypeBuilder<SistemBildirim> b)
        {
            base.Configure(b);
            b.Property(x => x.Kanal).HasConversion<int>();
            b.Property(x => x.Baslik).HasMaxLength(200);
            b.Property(x => x.Icerik).HasMaxLength(2000);
            b.Property(x => x.Durum).HasConversion<int>();
            b.HasIndex(x => new { x.Kanal, x.PlanlananTarih, x.Durum });
        }
    }

    public class OutboxMesajConfiguration : BaseEntityConfig<OutboxMesaj>
    {
        public override void Configure(EntityTypeBuilder<OutboxMesaj> b)
        {
            base.Configure(b);
            b.Property(x => x.Tip).IsRequired().HasMaxLength(100);
            b.Property(x => x.IcerikJson).IsRequired();
            b.Property(x => x.Islenmis).HasDefaultValue(false);
            b.HasIndex(x => new { x.Islenmis, x.CreateDate });
        }
    }

    public class WebhookAbonelikConfiguration : BaseEntityConfig<WebhookAbonelik>
    {
        public override void Configure(EntityTypeBuilder<WebhookAbonelik> b)
        {
            base.Configure(b);
            b.Property(x => x.Event).IsRequired().HasMaxLength(100);
            b.Property(x => x.Url).IsRequired().HasMaxLength(500);
            b.Property(x => x.IletiGizliAnahtar).HasMaxLength(200);
            b.HasOne(x => x.Bayi).WithMany(p => p.WebhookAbonelikleri).HasForeignKey(x => x.BayiId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.BayiId, x.Event }).IsUnique();
        }
    }
}

