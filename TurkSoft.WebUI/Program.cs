using TurkSoft.WebUI.AppSettings;
var builder = WebApplication.CreateBuilder(args);
//"Api" bölümünü Options olarak kaydet
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("Api"));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
// Add services to the container.
builder.Services.AddControllersWithViews();
// CORS policy ekle
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowNoxmusavir", policy =>
  {
    policy.WithOrigins("https://noxmusavir.com")
          .AllowAnyHeader()
          .AllowAnyMethod();
  });
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// CORS kullan
app.UseCors("AllowNoxmusavir");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LoginCover}/{action=Index}/{id?}");

app.Run();
