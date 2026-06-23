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
    var origins = new List<string>
    {
      "https://noxmusavir.com",
      "https://www.noxmusavir.com",
      "https://erpapi.noxmusavir.com"
    };
    if (builder.Environment.IsDevelopment())
    {
      origins.Add("http://localhost:5050");
      origins.Add("https://localhost:7228");
      origins.Add("http://localhost:5000");
      origins.Add("https://localhost:5001");
    }
    policy.WithOrigins(origins.ToArray())
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
