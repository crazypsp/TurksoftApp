using TurkSoft.GIBWebUI.AppSettings;

var builder = WebApplication.CreateBuilder(args);

// "Api" ayarlarını kaydet
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("Api"));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// 🔥 IHttpClientFactory kaydı (LoginController'da kullanıyorsun)
builder.Services.AddHttpClient();

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
app.UseAuthorization();

// Varsayılan route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
