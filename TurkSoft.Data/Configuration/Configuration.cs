// GEREKLİ USING'LER
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Data.Configuration
{
    // ORTAK ENTITY ALANLARI İÇİN FLUENT CONFIGURATION
    // Bu sınıf, tüm Entity'lerde ortak olan alanların yapılandırılmasını sağlar.
    // Tekrarlı kodlardan kurtulmak ve standart bir yapı oluşturmak amacıyla oluşturulmuştur.
    public static class EntityBaseConfiguration
    {
        public static void ConfigureBase<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : BaseEntity
        {
            // Id alanı Primary Key olarak ayarlanır
            builder.HasKey(x => x.Id);

            // Id alanı zorunlu olarak işaretlenir
            builder.Property(x => x.Id).IsRequired();

            // Oluşturulma tarihi zorunludur
            builder.Property(x => x.CreateDate).IsRequired();

            // Güncelleme tarihi isteğe bağlıdır (nullable)
            builder.Property(x => x.UpdateDate).IsRequired(false);

            // Silinme tarihi isteğe bağlıdır (nullable)
            builder.Property(x => x.DeleteDate).IsRequired(false);

            // IsActive varsayılan olarak true olur
            builder.Property(x => x.IsActive).HasDefaultValue(true);
        }
    }

    // KULLANICI TABLOSU CONFIGURATION
    // Sistem kullanıcılarını (admin, müşteri, destek vb.) tutmak için kullanılır
    public class KullaniciConfiguration : IEntityTypeConfiguration<Kullanici>
    {
        public void Configure(EntityTypeBuilder<Kullanici> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureBase(); // Ortak alanları uygular

            builder.Property(x => x.AdSoyad).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Eposta).IsRequired().HasMaxLength(150);
            builder.HasIndex(x => x.Eposta).IsUnique(); // Eposta benzersiz olmalı
            builder.Property(x => x.Sifre).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Telefon).HasMaxLength(20);
            builder.Property(x => x.Rol).HasMaxLength(50);
            builder.Property(x => x.ProfilResmiUrl).HasMaxLength(500);
        }
    }

    // MALİ MÜŞAVİR CONFIGURATION
    // Mali müşavirlerin kayıtlarını tutar. Her müşavir birden fazla firmaya sahip olabilir
    public class MaliMusavirConfiguration : IEntityTypeConfiguration<MaliMusavir>
    {
        public void Configure(EntityTypeBuilder<MaliMusavir> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureBase();

            builder.Property(x => x.AdSoyad).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Telefon).HasMaxLength(20);
            builder.Property(x => x.Eposta).HasMaxLength(150);
            builder.Property(x => x.Unvan).HasMaxLength(100);
            builder.Property(x => x.VergiNo).HasMaxLength(15);
            builder.Property(x => x.TCKN).HasMaxLength(11);
        }
    }

    // FİRMA CONFIGURATION
    // Mali müşavire bağlı firmaları temsil eder
    public class FirmaConfiguration : IEntityTypeConfiguration<Firma>
    {
        public void Configure(EntityTypeBuilder<Firma> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureBase();

            builder.Property(x => x.FirmaAdi).IsRequired().HasMaxLength(150);
            builder.Property(x => x.VergiNo).HasMaxLength(15);
            builder.Property(x => x.YetkiliAdSoyad).HasMaxLength(100);
            builder.Property(x => x.Telefon).HasMaxLength(20);
            builder.Property(x => x.Eposta).HasMaxLength(150);
            builder.Property(x => x.Adres).HasMaxLength(500);
        }
    }

    // LİSANS CONFIGURATION
    // Uygulama lisans bilgilerini tutar (başlangıç/bitiş tarihi ve lisans anahtarı)
    public class LisansConfiguration : IEntityTypeConfiguration<Lisans>
    {
        public void Configure(EntityTypeBuilder<Lisans> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureBase();

            builder.Property(x => x.LisansAnahtari).IsRequired().HasMaxLength(100);
        }
    }

    // LİSANS ADET CONFIGURATION
    // Lisans başına kurulu cihaz sayısını ve limitleri tutar
    public class LisansAdetConfiguration : IEntityTypeConfiguration<LisansAdet>
    {
        public void Configure(EntityTypeBuilder<LisansAdet> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureBase();
        }
    }

    // LOG CONFIGURATION
    // Uygulamada yapılan tüm işlemler bu tabloda loglanır
    public class LogConfiguration : IEntityTypeConfiguration<Log>
    {
        public void Configure(EntityTypeBuilder<Log> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureBase();

            builder.Property(x => x.Islem).IsRequired().HasMaxLength(255);
            builder.Property(x => x.IpAdres).HasMaxLength(50);
            builder.Property(x => x.Tarayici).HasMaxLength(100);
        }
    }

    // ÜRÜN TİPİ CONFIGURATION
    // Satılabilir ürünlerin kategorilerini (yazılım, servis vb.) tutar
    public class UrunTipiConfiguration : IEntityTypeConfiguration<UrunTipi>
    {
        public void Configure(EntityTypeBuilder<UrunTipi> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureBase();

            builder.Property(x => x.Ad).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Aciklama).HasMaxLength(300);
        }
    }

    // PAKET CONFIGURATION
    // Ürün tipine bağlı paketleri tanımlar (örnek: Başlangıç, Standart, Pro)
    public class PaketConfiguration : IEntityTypeConfiguration<Paket>
    {
        public void Configure(EntityTypeBuilder<Paket> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureBase();

            builder.Property(x => x.Ad).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Aciklama).HasMaxLength(300);
        }
    }

    // ÜRÜN FİYAT CONFIGURATION
    // Ürünlerin fiyatlarını ve geçerlilik tarihlerini tutar
    public class UrunFiyatConfiguration : IEntityTypeConfiguration<UrunFiyat>
    {
        public void Configure(EntityTypeBuilder<UrunFiyat> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureBase();

            builder.Property(x => x.Fiyat).HasColumnType("decimal(18,2)"); // Finansal veri tipi
            builder.Property(x => x.ParaBirimi).HasMaxLength(10);
        }
    }

    // SMS AYAR CONFIGURATION
    // SMS servis sağlayıcısına ait ayarlar
    public class SmsAyarConfiguration : IEntityTypeConfiguration<SmsAyar>
    {
        public void Configure(EntityTypeBuilder<SmsAyar> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureBase();

            builder.Property(x => x.ApiKey).IsRequired().HasMaxLength(150);
            builder.Property(x => x.ApiSecret).HasMaxLength(150);
            builder.Property(x => x.GondericiAdi).HasMaxLength(100);
        }
    }

    // WHATSAPP AYAR CONFIGURATION
    // Whatsapp entegrasyonu için gerekli ayarları içerir
    public class WhatsappAyarConfiguration : IEntityTypeConfiguration<WhatsappAyar>
    {
        public void Configure(EntityTypeBuilder<WhatsappAyar> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureBase();

            builder.Property(x => x.ApiUrl).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Token).HasMaxLength(500);
            builder.Property(x => x.Numara).HasMaxLength(20);
        }
    }

    // MAİL AYAR CONFIGURATION
    // Mail gönderimi için SMTP bilgilerini tutar
    public class MailAyarConfiguration : IEntityTypeConfiguration<MailAyar>
    {
        public void Configure(EntityTypeBuilder<MailAyar> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureBase();

            builder.Property(x => x.Eposta).IsRequired().HasMaxLength(150);
            builder.Property(x => x.SmtpServer).HasMaxLength(100);
            builder.Property(x => x.Sifre).HasMaxLength(100);
        }
    }

    // MAİL GÖNDERİM CONFIGURATION
    // Gönderilen maillere ait log bilgilerini tutar
    public class MailGonderimConfiguration : IEntityTypeConfiguration<MailGonderim>
    {
        public void Configure(EntityTypeBuilder<MailGonderim> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureBase();

            builder.Property(x => x.Alici).IsRequired().HasMaxLength(150);
            builder.Property(x => x.Konu).HasMaxLength(250);
        }
    }

    // WHATSAPP GÖNDERİM CONFIGURATION
    // Gönderilen WhatsApp mesajlarının kayıtlarını tutar
    public class WhatsappGonderimConfiguration : IEntityTypeConfiguration<WhatsappGonderim>
    {
        public void Configure(EntityTypeBuilder<WhatsappGonderim> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureBase();

            builder.Property(x => x.AliciNumara).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Mesaj).HasMaxLength(1000);
        }
    }

    // SMS GÖNDERİM CONFIGURATION
    // Gönderilen SMS kayıtlarını tutar
    public class SmsGonderimConfiguration : IEntityTypeConfiguration<SmsGonderim>
    {
        public void Configure(EntityTypeBuilder<SmsGonderim> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ConfigureBase();

            builder.Property(x => x.AliciNumara).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Mesaj).HasMaxLength(1000);
        }
    }

    // LUCA CONFIGURATION
    // Mali müşavirlerin Luca sistemine giriş yapabilmeleri için gerekli kullanıcı bilgilerini tutar.
    // Her Luca kaydı bir mali müşavire veya birden fazla mali müşavire bağlanabilir.
    public class LucaConfiguration : IEntityTypeConfiguration<Luca>
    {
        public void Configure(EntityTypeBuilder<Luca> builder)
        {
            // Id alanı Primary Key
            builder.HasKey(x => x.Id);
            // Ortak alanları uygular (Id, CreateDate, UpdateDate, DeleteDate, IsActive)
            builder.ConfigureBase();

            // Tablo adı
            builder.ToTable("Luca");

            // Üye numarası zorunlu, max 50 karakter
            builder.Property(x => x.UyeNo)
                   .IsRequired()
                   .HasMaxLength(50);

            // Kullanıcı adı zorunlu, max 100 karakter
            builder.Property(x => x.KullaniciAdi)
                   .IsRequired()
                   .HasMaxLength(100);

            // Parola zorunlu, max 200 karakter
            // (Not: gerçek hayatta hash/salt ile saklanmalı)
            builder.Property(x => x.Parola)
                   .IsRequired()
                   .HasMaxLength(200);

            // İlişki: Luca ↔ MaliMüşavir (çoktan çoğa)
            builder
                .HasMany(x => x.MaliMusavirs)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "LucaMaliMusavir", // Join tablo adı
                    right => right
                        .HasOne<MaliMusavir>()
                        .WithMany()
                        .HasForeignKey("MaliMusavirId")
                        .OnDelete(DeleteBehavior.Cascade),
                    left => left
                        .HasOne<Luca>()
                        .WithMany()
                        .HasForeignKey("LucaId")
                        .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.ToTable("Luca_MaliMusavir");
                        join.HasKey("LucaId", "MaliMusavirId");
                    }
                );
        }
    }

    // KEYACCOUNT CONFIGURATION
    // Mali müşavirler ile sistemdeki eşleştirme/bağlantı işlemlerinde kullanılan anahtar kelimeleri tutar.
    // Her KeyAccount kaydı birden fazla mali müşavire bağlanabilir.
    public class KeyAccountConfiguration : IEntityTypeConfiguration<KeyAccount>
    {
        public void Configure(EntityTypeBuilder<KeyAccount> builder)
        {
            // Id alanı Primary Key
            builder.HasKey(x => x.Id);
            // Ortak alanları uygular (Id, CreateDate, UpdateDate, DeleteDate, IsActive)
            builder.ConfigureBase();

            // Tablo adı
            builder.ToTable("KeyAccount");

            // Kod alanı zorunlu ve max 50 karakter
            builder.Property(x => x.Kod)
                   .IsRequired()
                   .HasMaxLength(50);

            // Açıklama alanı opsiyonel, max 300 karakter
            builder.Property(x => x.Aciklama)
                   .HasMaxLength(300);

            // Kod alanı benzersiz olmalı
            builder.HasIndex(x => x.Kod).IsUnique();

            // İlişki: KeyAccount ↔ MaliMüşavir (çoktan çoğa)
            builder
                .HasMany(x => x.MaliMusavirs)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "KeyAccountMaliMusavir", // Join tablo adı
                    right => right
                        .HasOne<MaliMusavir>()
                        .WithMany()
                        .HasForeignKey("MaliMusavirId")
                        .OnDelete(DeleteBehavior.Cascade),
                    left => left
                        .HasOne<KeyAccount>()
                        .WithMany()
                        .HasForeignKey("KeyAccountId")
                        .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.ToTable("KeyAccount_MaliMusavir");
                        join.HasKey("KeyAccountId", "MaliMusavirId");
                    }
                );
        }
    }
}

