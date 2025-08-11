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
// WebUI origin’in: https://localhost:7228 (gerekirse http varyantýný da ekleyebilirsin)
const string CorsPolicyName = "AllowWebUI";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7228"  // WebUI
                                          // ,"http://localhost:7228" // UI http kullanýyorsa aç
            )
            .AllowAnyMethod()     // POST/GET/OPTIONS vs.
            .AllowAnyHeader()     // Content-Type, Authorization vs.
                                  // .AllowCredentials() // Cookie/tabanlý auth gerekiyorsa aç; açarsan WithOrigins zorunlu (AllowAnyOrigin ile birlikte kullanýlamaz)
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

// !!! CORS middleware'ini Authorization'dan ÖNCE çaðýr
app.UseCors(CorsPolicyName);

app.UseAuthorization();

app.MapControllers();

app.Run();
