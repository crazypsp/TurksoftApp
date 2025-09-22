using Microsoft.OpenApi.Models;
using Serilog;
using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers;
using TurkSoft.Service.Interface;
using TurkSoft.Service.Manager;

var builder = WebApplication.CreateBuilder(args);

// Serilog (opsiyonel)
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

// Controller & DI servisleri
builder.Services.AddControllers();
builder.Services.AddAuthorization();
// ✅ SERVICE / BUSINESS INJECTION(EN ÖNEMLİSİ BU!)
builder.Services.AddScoped<IBankaEkstreAnalyzerService, BankaEkstreAnalyzerManagerSrv>();
builder.Services.AddScoped<IBankaEkstreAnalyzerBusiness, BankaEkstreAnalyzerManager>();

// ✅ Swagger tanımı (OpenAPI 3.0)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TurkSoft Matching API",
        Version = "3.0.0", // ✅ En önemli satır. "v1" yazarsan hata alırsın
        Description = "TurkSoft banka mutabakat API servisleri"
    });
});

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "https://localhost:7228" };

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

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TurkSoft Matching API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("WebUICors");
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
