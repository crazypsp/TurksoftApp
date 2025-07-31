using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers;
using TurkSoft.Service.Interface;
using TurkSoft.Service.Manager;
using Microsoft.OpenApi.Models;
using TurkSoft.Entities.Document;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------
// ✅ Service ve Business Katmanı Bağlantısı
// DI (Dependency Injection) ile sınıflar sisteme tanıtılıyor
// ---------------------------------------------------
builder.Services.AddScoped<IBankaEkstreService, BankaEkstreManagerSrv>();
builder.Services.AddScoped<IBankaEkstreBusiness, BankaEkstreManager>();

// ---------------------------------------------------
// ✅ Controller servisini DI ile ekle
// ---------------------------------------------------
builder.Services.AddControllers();

// ---------------------------------------------------
// ✅ Swagger/OpenAPI servislerini tanımla
// ---------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddSwaggerGen(options =>
//{
//    options.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "TurkSoft Banka Ekstre API",
//        Version = "v1",
//        Description = "Excel, PDF ve TXT Banka ekstre işlemleri için Web API"
//    });
//});

// ---------------------------------------------------
// ✅ Uygulama nesnesi oluşturuluyor
// ---------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------
// ✅ Hata ayıklama ve Swagger UI yapılandırması
// Geliştirme ortamında detaylı hataları göster
// ---------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ---------------------------------------------------
// HTTPS yönlendirme ve yetkilendirme
// ---------------------------------------------------
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ---------------------------------------------------
// ✅ Uygulama çalıştırılır
// ---------------------------------------------------
app.Run();
