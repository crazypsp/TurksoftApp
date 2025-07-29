using Microsoft.EntityFrameworkCore;                 // EF Core için gerekli
using TurkSoft.Data.Context;                         // AppDbContext erişimi için gerekli
using TurkSoft.Service.Interface;
using TurkSoft.Service.Manager;
using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers;
using TurkSoft.Entities.EntityDB;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// 🔗 Veritabanı bağlantı ayarları
// appsettings.json içindeki "DefaultConnection" key'ini kullanarak bağlanır
// Bu DbContext, EF Core üzerinden SQL Server'a bağlantı sağlar
// --------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Service ve Business Katmanı Bağlantısı
builder.Services.AddScoped<IKullaniciService, KullaniciManager>();
builder.Services.AddScoped<IBaseService<Kullanici>, BaseManager<Kullanici>>();

builder.Services.AddScoped<IFirmaService, FirmaManager>();
builder.Services.AddScoped<IBaseService<Firma>, BaseManager<Firma>>();

builder.Services.AddScoped<ILisansService, LisansManager>();
builder.Services.AddScoped<IBaseService<Lisans>, BaseManager<Lisans>>();

builder.Services.AddScoped<ILisansAdetService, LisansAdetManager>();
builder.Services.AddScoped<IBaseService<LisansAdet>, BaseManager<LisansAdet>>();

builder.Services.AddScoped<ILogService, LogManager>();
builder.Services.AddScoped<IBaseService<Log>, BaseManager<Log>>();

builder.Services.AddScoped<IMailAyarService, MailAyarManager>();
builder.Services.AddScoped<IBaseService<MailAyar>, BaseManager<MailAyar>>();

builder.Services.AddScoped<IMailGonderimService, MailGonderimManager>();
builder.Services.AddScoped<IBaseService<MailGonderim>, BaseManager<MailGonderim>>();

builder.Services.AddScoped<IMaliMusavirService, MaliMusavirManager>();
builder.Services.AddScoped<IBaseService<MaliMusavir>, BaseManager<MaliMusavir>>();

builder.Services.AddScoped<IPaketService, PaketManager>();
builder.Services.AddScoped<IBaseService<Paket>, BaseManager<Paket>>();

builder.Services.AddScoped<ISmsAyarService, SmsAyarManager>();
builder.Services.AddScoped<IBaseService<SmsAyar>, BaseManager<SmsAyar>>();

builder.Services.AddScoped<ISmsGonderimService, SmsGonderimManager>();
builder.Services.AddScoped<IBaseService<SmsGonderim>, BaseManager<SmsGonderim>>();

builder.Services.AddScoped<IUrunFiyatService, UrunFiyatManager>();
builder.Services.AddScoped<IBaseService<UrunFiyat>, BaseManager<UrunFiyat>>();

builder.Services.AddScoped<IUrunTipiService, UrunTipiManager>();
builder.Services.AddScoped<IBaseService<UrunTipi>, BaseManager<UrunTipi>>();

builder.Services.AddScoped<IWhatsappAyarService, WhatsappAyarManager>();
builder.Services.AddScoped<IBaseService<WhatsappAyar>, BaseManager<WhatsappAyar>>();

builder.Services.AddScoped<IWhatsappGonderimService, WhatsappGonderimManager>();
builder.Services.AddScoped<IBaseService<WhatsappGonderim>, BaseManager<WhatsappGonderim>>();

// --------------------------------------------------
// ⚙️ Controller servisini DI (Dependency Injection) ile ekliyoruz
// API controller'ların aktif olarak çalışmasını sağlar
// --------------------------------------------------
builder.Services.AddControllers();

// --------------------------------------------------
// 🔎 Swagger/OpenAPI desteği
// API endpoint'lerini UI üzerinden test etmeyi sağlar
// --------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// --------------------------------------------------
// 🔧 Uygulama yapılandırması tamamlandıktan sonra app nesnesi oluşturulur
// --------------------------------------------------
var app = builder.Build();


// --------------------------------------------------
// 🔄 Geliştirme ortamındaysa Swagger arayüzü aktif edilir
// Bu, sadece Develop ortamında Swagger UI'ı gösterir
// --------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --------------------------------------------------
// 🌐 HTTPS yönlendirmesi aktif edilir (SSL güvenliği)
// --------------------------------------------------
app.UseHttpsRedirection();

// --------------------------------------------------
// 🔐 Yetkilendirme middleware’i (ileride token veya kimlik doğrulama yapılırsa kullanılır)
// --------------------------------------------------
app.UseAuthorization();

// --------------------------------------------------
// 🗺️ Controller'lara yönlendirme tanımlanır
// Route tanımına uygun gelen istekler ilgili controller'a gider
// --------------------------------------------------
app.MapControllers();

// --------------------------------------------------
// 🚀 Uygulama çalıştırılır
// --------------------------------------------------
app.Run();