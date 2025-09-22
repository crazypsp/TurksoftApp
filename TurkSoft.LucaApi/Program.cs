using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers;
using TurkSoft.Service.Interface;
using TurkSoft.Service.Manager;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// LOGGING (Serilog)
// -------------------------
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

// -------------------------
// DI (Servis Kayıtları)
// -------------------------
builder.Services.AddScoped<ILucaAutomationService, LucaAutomationManagerSrv>();
builder.Services.AddScoped<ILucaAutomationBussiness, LucaAutomationManager>();

builder.Services.AddControllers();

// -------------------------
// Swagger
// -------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TurkSoft Luca API",
        Version = "v1",
        Description = "Luca muhasebe entegrasyon servisi"
    });
});

// -------------------------
// CORS AYARI
// -------------------------
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "https://localhost:7228" };

// Web uygulamasının eriştiği domaini buraya yaz (tam adres: https + domain)
var uiOrigin = "https://noxmusavir.com";

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebUICors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// -------------------------
// HTTPS / Proxy için ForwardedHeaders
// -------------------------
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// -------------------------
// Middleware Pipeline
// -------------------------

app.UseForwardedHeaders();

// HSTS yalnızca prod ortamda aktif edilir
if (app.Environment.IsProduction())
{
    app.UseHsts();
}

// Swagger (her ortamda aktif olsun)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Luca API v1");
    c.RoutePrefix = "swagger"; // https://lucaapi.noxmusavir.com/swagger
    c.DisplayRequestDuration();
});

// ✅ HTTPS yönlendirmesi sonrası CORS'u UYGULA!
app.UseHttpsRedirection();

app.UseCors("WebUICors");

// Eğer authentication varsa önce onu çalıştırman gerekir
// app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Swagger root yönlendirmesi
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
