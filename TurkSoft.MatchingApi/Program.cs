using TurkSoft.Business.Interface;
using TurkSoft.Service.Interface;
using TurkSoft.Service.Manager;

var builder = WebApplication.CreateBuilder(args);

// Service & Business
builder.Services.AddScoped<IBankaEkstreAnalyzerService, BankaEkstreAnalyzerManagerSrv>();
builder.Services.AddScoped<IBankaEkstreAnalyzerBusiness, BankaEkstreAnalyzerManager>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ******** CORS ********
// WebUI origin�in: https://localhost:7228 (gerekirse http varyant�n� da ekleyebilirsin)
const string CorsPolicyName = "AllowWebUI";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7228"  // WebUI
                                          // ,"http://localhost:7228" // UI http kullan�yorsa a�
            )
            .AllowAnyMethod()     // POST/GET/OPTIONS vs.
            .AllowAnyHeader()     // Content-Type, Authorization vs.
                                  // .AllowCredentials() // Cookie/tabanl� auth gerekiyorsa a�; a�arsan WithOrigins zorunlu (AllowAnyOrigin ile birlikte kullan�lamaz)
            .SetPreflightMaxAge(TimeSpan.FromHours(1)); // opsiyonel: preflight cache
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// !!! CORS middleware'ini Authorization'dan �NCE �a��r
app.UseCors(CorsPolicyName);

app.UseAuthorization();

app.MapControllers();

app.Run();
