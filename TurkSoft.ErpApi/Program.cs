using Microsoft.EntityFrameworkCore;                 // EF Core için gerekli
using TurkSoft.Data.Context;                         // AppDbContext erişimi için gerekli

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// 🔗 Veritabanı bağlantı ayarları
// appsettings.json içindeki "DefaultConnection" key'ini kullanarak bağlanır
// Bu DbContext, EF Core üzerinden SQL Server'a bağlantı sağlar
// --------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


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