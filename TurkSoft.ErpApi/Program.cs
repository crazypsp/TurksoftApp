using System.Reflection;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using TurkSoft.Data.Context;
using TurkSoft.Service;

var builder = WebApplication.CreateBuilder(args);

// -------------------- SERVICES --------------------

// 1) DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    )
);

// 2) Servis katmanı
builder.Services.AddEntityServices();

// 3) Controllers + JSON güvenli varsayılanlar
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        o.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// 4) API Versioning
builder.Services
    .AddApiVersioning(o =>
    {
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.ReportApiVersions = true;
    })
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });

// 5) Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IConfigureOptions<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>,
                              ConfigureSwaggerOptions>();

// 6) HealthChecks
builder.Services.AddHealthChecks()
    .AddCheck<TurkSoft.ErpApi.HealthChecks.DbContextHealthCheck>("sql");

// 7) CORS
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(o =>
{
    o.AddPolicy("WebUICors", p =>
    {
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials();
    });
});

// 8) ProblemDetails
builder.Services.AddProblemDetails();

// 9) Upload limiti (çok büyük dosyalar için)
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = long.MaxValue;
});

// 10) HTTPS için Proxy ayarı (Plesk/Nginx/IIS vs.)
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// -------------------- PIPELINE --------------------

// Hata sayfası geliştirme ortamında
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseForwardedHeaders();

if (app.Environment.IsProduction())
{
    app.UseHsts(); // prod ortamı için zorunlu değil ama önerilir
}

app.UseHttpsRedirection();

// CORS middleware (politikayı belirt)
app.UseCors("WebUICors");

// Exception middleware (Swagger ve Health dışı yollar için)
app.UseWhen(ctx =>
    !ctx.Request.Path.StartsWithSegments("/swagger") &&
    !ctx.Request.Path.StartsWithSegments("/health"),
    branch =>
    {
        branch.UseMiddleware<TurkSoft.ErpApi.Middleware.GlobalExceptionMiddleware>();
    });

// Swagger
app.UseSwagger();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
app.UseSwaggerUI(c =>
{
    foreach (var desc in provider.ApiVersionDescriptions)
    {
        c.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",
            $"TurkSoft ERP API {desc.GroupName.ToUpperInvariant()}");
    }
    c.RoutePrefix = builder.Configuration["Swagger:RoutePrefix"] ?? "swagger";
    c.DisplayRequestDuration();
});

// app.UseAuthentication(); // Eğer kimlik doğrulama eklenirse
// app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Root URL => Swagger'a yönlendir
app.MapGet("/", () =>
    Results.Redirect("/" + (builder.Configuration["Swagger:RoutePrefix"] ?? "swagger"))
);

app.Run();


// -------------------- Swagger Options (Multi-Version Support) --------------------
public sealed class ConfigureSwaggerOptions
    : IConfigureOptions<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

    public void Configure(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        foreach (var desc in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(desc.GroupName, new OpenApiInfo
            {
                Title = "TurkSoft ERP API",
                Version = desc.ApiVersion.ToString(),
                Description = "TurkSoft ERP/CRM servisleri"
            });
        }

        options.DocInclusionPredicate((docName, apiDesc) =>
            apiDesc.GroupName == docName && apiDesc.HttpMethod != null);

        options.ResolveConflictingActions(apiDescs => apiDescs.First());
        options.CustomSchemaIds(t => t.FullName!.Replace('+', '.'));
        options.SupportNonNullableReferenceTypes();
        options.IgnoreObsoleteActions();
        options.IgnoreObsoleteProperties();

        var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
}
