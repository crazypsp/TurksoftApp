using TurkSoft.BankWebUI.AppSettings;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("Api"));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
// Add services to the container.
builder.Services.AddControllersWithViews();
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
app.UseCors("AllowNoxmusavir");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
