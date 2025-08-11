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
// CORS
// -------------------------
const string WebUiCorsPolicy = "AllowWebUI";
// UI origin'in tam adresi (proto + host + port)
var uiOrigin = "https://localhost:7228";

builder.Services.AddCors(options =>
{
    options.AddPolicy(WebUiCorsPolicy, policy =>
    {
        policy.WithOrigins(uiOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              // Cookie/Session veya Authorization header ile �al���yorsan a��k kals�n:
              .AllowCredentials();
        // NOT: AllowCredentials() ile '*' kullan�lamaz, tek tek origin yaz�lmal�.
    });
});

var app = builder.Build();

// -------------------------
// Middleware Pipeline
// -------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS, Authorization'dan ve MapControllers'tan �NCE olmal�
app.UseCors(WebUiCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
