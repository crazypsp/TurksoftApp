using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TurkSoft.Data.GibData;          // GibAppDbContext
using TurkSoft.Service;               // AddEntityServices / AddGibEntityServices
using TurkSoft.Service.Interface;
using TurkSoft.Service.Manager;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Controllers & Swagger ----------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TurkSoft.GibPortalApi",
        Version = "v1"
    });

    // Nested type isim çakışmalarını engelle
    c.CustomSchemaIds(t => t.FullName?.Replace("+", "."));
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

// ---------------- DbContext (GibAppDbContext) ----------------
builder.Services.AddDbContext<GibAppDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("GibEntityDb");
    if (string.IsNullOrWhiteSpace(connStr))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:GibEntityDb appsettings içinde tanımlı değil.");
    }

    options.UseSqlServer(connStr);
});

// ---------------- Business & Entity servisleri ----------------
builder.Services.AddScoped<IGibBusiness, GibBusiness>();
builder.Services.AddGibEntityServices();   // TurkSoft.Service.GibServiceRegistrationExtensions

// ---------------- CORS ----------------
// WebUI projen https://localhost:7056 üzerinde çalışıyor (layout'ta öyle kullanıyorsun).
// Gerekirse buraya http://localhost:7056 veya farklı portları da ekleyebilirsin.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebUi", policy =>
    {
        policy
            .WithOrigins("https://localhost:7056") // Web UI origin
            .AllowAnyHeader()
            .AllowAnyMethod();
        // .AllowCredentials(); // Cookie / auth gerekiyorsa açarsın
    });
});

var app = builder.Build();

// ---------------- Middleware Pipeline ----------------

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Swagger'ı her ortamda açmak istersen aşağıdaki iki satırı if dışına da alabilirsin:
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TurkSoft.GibPortalApi v1");
});

// HTTPS
app.UseHttpsRedirection();

// Router'ı devreye al
app.UseRouting();

// CORS'u routing'ten sonra, auth'tan önce kullan
app.UseCors("AllowWebUi");

// Eğer authentication kullanıyorsan, buraya app.UseAuthentication() ekle
// app.UseAuthentication();

app.UseAuthorization();

// Endpoint mapping
app.MapControllers();

app.Run();
