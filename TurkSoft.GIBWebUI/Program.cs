using TurkSoft.GIBWebUI.AppSettings;

var builder = WebApplication.CreateBuilder(args);

// "Api" ayarlar�n� kaydet
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("Api"));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// MVC / Razor deste�i
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

// Varsay�lan route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LoginCover}/{action=Index}/{id?}");

app.Run();
