using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers;
using TurkSoft.Service.Interface;
using TurkSoft.Service.Manager;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------
// ✅ Service ve Business Katmanı Bağlantısı (Dependency Injection ile tanımlama)
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
// (İsteğe bağlı Swagger yapılandırması buraya eklenebilir)

// ---------------------------------------------------
// ✅ CORS politikası tanımlama (AllowAll adında bir politika)
// Bu satırı builder.Build() ÇAĞRILMADAN ÖNCE ekleyin
// ---------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
        policyBuilder.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader());
});

// ---------------------------------------------------
// ✅ Uygulama oluşturuluyor
// ---------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------
// ✅ Geliştirme ortamında Swagger UI etkinleştir
// ---------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ---------------------------------------------------
// ✅ HTTP istek işleme ardışık düzeni (middleware pipeline)
// ---------------------------------------------------
app.UseHttpsRedirection();

// CORS politikasını uygulama (AllowAll politikasını kullan)
// Bunu Authorization'dan ÖNCE çağırmak gerekir:
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

// ---------------------------------------------------
// ✅ Uygulamayı çalıştır
// ---------------------------------------------------
app.Run();
