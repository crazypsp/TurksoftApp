using Microsoft.OpenApi.Models;
using Turksoft.BankServiceAPI.Security;
using TurkSoft.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger + ApiKey header desteði
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Turksoft.BankServiceAPI", Version = "v1" });

    // X-API-KEY için Swagger Security
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

// Service katmaný (içeride Business kayýtlarýný da yapýyor)
builder.Services.AddTurkSoftServices();

// Ýstersen ileride [Authorize] kullanýrsýn diye kalsýn
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ApiKey kontrolü (Authorization gibi)
app.UseMiddleware<ApiKeyMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.Run();
