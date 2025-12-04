using System.IO;
using Microsoft.AspNetCore.DataProtection;
using TurkSoft.GIBWebUI.AppSettings;

var builder = WebApplication.CreateBuilder(args);

// "Api" ayarlarını kaydet
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("Api"));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// IHttpClientFactory
builder.Services.AddHttpClient();

// 🔐 DataProtection key'lerini diske persist et (antiforgery hatası için KRİTİK)
var keysDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys");
Directory.CreateDirectory(keysDir);

builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysDir))
    .SetApplicationName("TurkSoft.GIBWebUI");

// Antiforgery cookie ismini değiştir – eski bozuk cookie’ler devre dışı kalsın
builder.Services.AddAntiforgery(o =>
{
    o.Cookie.Name = "Nox.Xsrf"; // eskisinden farklı bir isim
});

// MVC / Razor desteği
builder.Services.AddControllersWithViews();

var app = builder.Build();

// =============================
// PIPELINE
// =============================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // varsa
app.UseAuthorization();

// Varsayılan route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
