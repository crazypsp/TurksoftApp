using Microsoft.Extensions.Logging;
using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers;
using TurkSoft.Service.Interface;
using TurkSoft.Service.Manager;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------
// ✅ Logging Ayarları (Konsol + Dosya Loglama)
// ---------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // Terminal veya Output penceresine log
builder.Logging.AddDebug();   // Visual Studio output için
builder.Logging.AddFile("Logs/log-{Date}.txt"); // Her güne özel dosya loglama (Serilog.Extensions.Logging.File paketi gerekir)

// ---------------------------------------------------
// ✅ Service ve Business Katmanı Bağlantısı
// ---------------------------------------------------
builder.Services.AddScoped<IBankaEkstreService, BankaEkstreManagerSrv>();
builder.Services.AddScoped<IBankaEkstreBusiness, BankaEkstreManager>();

builder.Services.AddControllers();

// ---------------------------------------------------
// ✅ Swagger/OpenAPI servisleri
// ---------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------------------------------------------------
// ✅ CORS Politikası Tanımı (Yayın ortamındaki domainler dahil)
// ---------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
        policyBuilder
            .WithOrigins(
                "https://documentapi.noxmusavir.com",
                "https://noxmusavir.com",
                "https://www.noxmusavir.com",
                "http://documentapi.noxmusavir.com",
                "http://noxmusavir.com",
                "http://www.noxmusavir.com",
                "https://localhost:7228",
                "http://localhost:7228"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
    );
});

// ---------------------------------------------------
// ✅ Uygulama Yapılandırma
// ---------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------
// ✅ Swagger – Yayın ortamında da aktif olabilir (opsiyonel)
// ---------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Opsiyonel: Yayın ortamında da Swagger açılsın istiyorsan burayı aç
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Document API v1");
        c.RoutePrefix = ""; // Direkt domain yazınca swagger çalışsın
    });
}

// ---------------------------------------------------
// ✅ HTTP Middleware Pipeline
// ---------------------------------------------------
app.UseHttpsRedirection();

// ✅ CORS: Authorization'dan önce
app.UseCors("AllowAll");

app.UseAuthorization();

// ✅ Controller yönlendirme
app.MapControllers();

// ---------------------------------------------------
// ✅ Uygulamayı Başlat
// ---------------------------------------------------
app.Run();
