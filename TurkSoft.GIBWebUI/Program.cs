using TurkSoft.GIBWebUI.AppSettings;

var builder = WebApplication.CreateBuilder(args);

// "Api" ayarlarýný kaydet
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("Api"));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// MVC / Razor desteði
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

// Varsayýlan route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
