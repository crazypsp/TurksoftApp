using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers;
using TurkSoft.Service.Interface;
using TurkSoft.Service.Manager;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// DI: Service & Business
// -------------------------
builder.Services.AddScoped<IBankaEkstreService, BankaEkstreManagerSrv>();
builder.Services.AddScoped<IBankaEkstreBusiness, BankaEkstreManager>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------
// CORS (UI'den gelen istekler için izin ver)
// -------------------------
// Not: AllowCredentials() kullanıyorsan kesinlikle WithOrigins ile tek tek belirtmen gerekir.
const string CorsPolicyName = "AllowWebUI";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policyBuilder =>
    {
        policyBuilder
            .WithOrigins("https://noxmusavir.com") // UI domain (http varyantı gerekiyorsa ekle)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Tarayıcıdan session/cookie/token geliyorsa bu şart
    });
});

var app = builder.Build();

// -------------------------
// Swagger (dev ortamı için aktif)
// -------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ CORS middleware'ini Authorization'dan ÖNCE çağır
app.UseCors(CorsPolicyName);

app.UseAuthorization();

app.MapControllers();

app.Run();
