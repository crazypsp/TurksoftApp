using Microsoft.OpenApi.Models;
using Turksoft.BankServiceAPI.Security;
using TurkSoft.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✅ CORS (AllowedOrigins: appsettings.json -> Cors:AllowedOrigins)
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("TS_CORS", p =>
    {
        // Origin bazlı izin (localhost dev)
        if (allowedOrigins.Length > 0)
            p.WithOrigins(allowedOrigins);
        else
            p.SetIsOriginAllowed(_ => true); // fallback (tavsiye edilmez)

        p.AllowAnyHeader()
         .AllowAnyMethod();
    });
});

// Swagger + ApiKey header desteği
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Turksoft.BankServiceAPI", Version = "v1" });

    var headerName = builder.Configuration["ApiKey:HeaderName"] ?? "X-API-KEY";

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = $"API Key giriniz. Header: {headerName}",
        Name = headerName,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
            },
            Array.Empty<string>()
        }
    });
});

// ApiKey options + middleware
builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection("ApiKey"));
builder.Services.AddScoped<ApiKeyMiddleware>();

// Service katmanı
builder.Services.AddTurkSoftServices();

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ Routing + CORS sırası önemli
app.UseRouting();
app.UseCors("TS_CORS");

// ✅ ApiKey kontrolü CORS’tan sonra olmalı (preflight için)
app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthorization();

app.MapControllers();
app.Run();
