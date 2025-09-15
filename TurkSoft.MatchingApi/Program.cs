using TurkSoft.Business.Interface;
using TurkSoft.Service.Interface;
using TurkSoft.Service.Manager;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// DI
// -------------------------
builder.Services.AddScoped<IBankaEkstreAnalyzerService, BankaEkstreAnalyzerManagerSrv>();
builder.Services.AddScoped<IBankaEkstreAnalyzerBusiness, BankaEkstreAnalyzerManager>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------
// CORS
// -------------------------
// Web UI domaini (tam adres ile)
// Örn: "https://noxmusavir.com"
const string CorsPolicyName = "AllowWebUI";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins("https://noxmusavir.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials() // ❗ Tarayıcıdan cookie/token ile istek varsa bu ŞART
              .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});

var app = builder.Build();

// -------------------------
// Middleware Pipeline
// -------------------------

// Swagger sadece development ortamında
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ CORS middleware'ini mutlaka Authorization'dan ÖNCE çağır
app.UseCors(CorsPolicyName);

// Eğer auth kullanıyorsan önce authentication sonra authorization
// app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
