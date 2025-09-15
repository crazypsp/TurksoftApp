using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers;
using TurkSoft.Service.Interface;
using TurkSoft.Service.Manager;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// DI
// -------------------------
builder.Services.AddScoped<ILucaAutomationService, LucaAutomationManagerSrv>();
builder.Services.AddScoped<ILucaAutomationBussiness, LucaAutomationManager>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------
// CORS AYARI
// -------------------------
const string WebUiCorsPolicy = "AllowWebUI";

// Web uygulamasının eriştiği domaini buraya yaz (tam adres: https + domain)
var uiOrigin = "https://noxmusavir.com";

builder.Services.AddCors(options =>
{
    options.AddPolicy(WebUiCorsPolicy, policy =>
    {
        policy.WithOrigins(uiOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Cookie / token taşıyorsan bu şart
    });
});

var app = builder.Build();

// -------------------------
// Middleware Pipeline
// -------------------------

// Swagger dev modunda aktif
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ HTTPS yönlendirmesi sonrası CORS'u UYGULA!
app.UseHttpsRedirection();

// ✅ CORS MIDDLEWARE - En kritik satır bu
app.UseCors(WebUiCorsPolicy);

// Eğer authentication varsa önce onu çalıştırman gerekir
// app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
