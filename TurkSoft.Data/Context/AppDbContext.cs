// GEREKLİ USING'LER
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Data.Context
{
    // Uygulamanın veri tabanı işlemlerini yöneten temel DbContext sınıfıdır.
    // Tüm Entity sınıfları bu context üzerinden yönetilir ve EF Core tarafından otomatik olarak migrasyon işlemlerinde kullanılır.
    public class AppDbContext : DbContext
    {
        // DbContext'e dışarıdan (örneğin Startup.cs içinden) gelen bağlantı ayarlarını alır
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Veri tabanındaki tabloların her biri için DbSet oluşturulmuştur.
        // Her DbSet EF Core tarafından ilgili tabloya karşılık gelir
        public DbSet<Kullanici> Kullanicilar { get; set; }             // Kullanıcı bilgileri
        public DbSet<Firma> Firmalar { get; set; }                     // Firmalar
        public DbSet<Lisans> Lisanslar { get; set; }                   // Lisanslar
        public DbSet<LisansAdet> LisansAdetler { get; set; }           // Lisans adetleri (kaç cihaz kurulmuş vs.)
        public DbSet<Log> Loglar { get; set; }                         // İşlem logları
        public DbSet<MailAyar> MailAyarlar { get; set; }               // Mail yapılandırma ayarları
        public DbSet<MailGonderim> MailGonderimler { get; set; }       // Gönderilen maillerin kayıtları
        public DbSet<MaliMusavir> MaliMusavirler { get; set; }         // Mali müşavir kayıtları
        public DbSet<Paket> Paketler { get; set; }                     // Ürün paketleri
        public DbSet<SmsAyar> SmsAyarlar { get; set; }                 // SMS API ayarları
        public DbSet<SmsGonderim> SmsGonderimler { get; set; }         // Gönderilen SMS kayıtları
        public DbSet<UrunFiyat> UrunFiyatlar { get; set; }             // Ürün fiyatlandırmaları
        public DbSet<UrunTipi> UrunTipiler { get; set; }               // Ürün tipi tanımları
        public DbSet<WhatsappAyar> WhatsappAyarlar { get; set; }       // WhatsApp API ayarları
        public DbSet<WhatsappGonderim> WhatsappGonderimler { get; set; } // WhatsApp gönderim logları

        // Fluent API konfigürasyonları burada uygulanır
        // Configuration klasöründe yer alan her entity konfigürasyon sınıfı burada tanıtılır
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // base metodun çağrılması önemlidir çünkü EF Core'un default davranışları korunur
            base.OnModelCreating(modelBuilder);

            // Assembly içinde bulunan tüm IEntityTypeConfiguration implementasyonlarını otomatik uygular
            // Bu sayede her bir entity için manuel olarak ApplyConfiguration yazmaya gerek kalmaz
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
