// Program.cs  (.NET 8, Asp.Versioning 8.1 uyumlu)

using System.Reflection;
using System.Text.Json.Serialization;
using Asp.Versioning;                     // ApiVersion, AddApiVersioning()
using Asp.Versioning.ApiExplorer;        // IApiVersionDescriptionProvider, AddApiExplorer()
using Microsoft.AspNetCore.Http.Features;   // FormOptions
using Microsoft.AspNetCore.HttpOverrides;   // ForwardedHeaders
using Microsoft.EntityFrameworkCore;        // UseSqlServer
using Microsoft.Extensions.Options;         // IConfigureOptions<T>
using Microsoft.OpenApi.Models;             // OpenApiInfo
using TurkSoft.Data.Context;                // AppDbContext
using TurkSoft.Service;                     // AddEntityServices()

var builder = WebApplication.CreateBuilder(args);

// -------------------- SERVICES --------------------

// 1) DbContext
// >>> DbContext kaydı (BURAYA) <<<
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

// 4) API Versioning (+ Explorer)
// NOT: AddVersionedApiExplorer artık yok; doğru kullanım budur. (8.1)
builder.Services
    .AddApiVersioning(o =>
    {
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.ReportApiVersions = true;
    })
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";          // v1, v1.1, v2
        o.SubstituteApiVersionInUrl = true;    // route’daki {version} yerini doldur
    });

// 5) Swagger (versiyon-farkında kurulum)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IConfigureOptions<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>,
                              ConfigureSwaggerOptions>();

// 6) HealthChecks
builder.Services.AddHealthChecks()
    .AddCheck<TurkSoft.ErpApi.HealthChecks.DbContextHealthCheck>("sql");

// 7) CORS
builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

// 8) ProblemDetails
builder.Services.AddProblemDetails();

// 9) (opsiyonel) Büyük upload
builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = long.MaxValue);

// 10) ForwardedHeaders (proxy/https arkasında doğru scheme/host)
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// -------------------- PIPELINE --------------------

// Geliştirmede ayrıntılı hata sayfası: swagger.json’daki istisneleri net görürsün
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseForwardedHeaders();

if (app.Environment.IsProduction())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseCors();

// ⛔ ÖNEMLİ: GlobalExceptionMiddleware’i Swagger & Health dışına al
app.UseWhen(ctx =>
    !ctx.Request.Path.StartsWithSegments("/swagger") &&
    !ctx.Request.Path.StartsWithSegments("/health"),
    branch =>
    {
        branch.UseMiddleware<TurkSoft.ErpApi.Middleware.GlobalExceptionMiddleware>();
    });

// Swagger JSON + UI (çoklu versiyon desteği)
app.UseSwagger();

var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
app.UseSwaggerUI(c =>
{
    foreach (var desc in provider.ApiVersionDescriptions)
    {
        c.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",
            $"TurkSoft ERP API {desc.GroupName.ToUpperInvariant()}");
    }

    // appsettings.json: "Swagger:RoutePrefix": "swagger"
    c.RoutePrefix = builder.Configuration["Swagger:RoutePrefix"] ?? "swagger";
    c.DisplayRequestDuration();
});

// (Authentication/Authorization kullanıyorsan burada ekle)
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Kökü Swagger UI'a yönlendir
app.MapGet("/", () => Results.Redirect("/" + (builder.Configuration["Swagger:RoutePrefix"] ?? "swagger")));

app.Run();


// -------------------- Swagger Options (versiyonlara göre) --------------------
public sealed class ConfigureSwaggerOptions
    : IConfigureOptions<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

    public void Configure(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        // Her API versiyonu için ayrı doküman
        foreach (var desc in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(desc.GroupName, new OpenApiInfo
            {
                Title = "TurkSoft ERP API",
                Version = desc.ApiVersion.ToString(),
                Description = "TurkSoft ERP/CRM servisleri"
            });
        }

        // Swagger JSON üretiminde 500'e yol açan tipik durumlara karşı korumalar:
        options.DocInclusionPredicate((docName, apiDesc) =>
            apiDesc.GroupName == docName && apiDesc.HttpMethod != null);     // HttpMethod null ise hariç
        options.ResolveConflictingActions(apiDescs => apiDescs.First());     // Çakışan route+method -> ilkini al
        options.CustomSchemaIds(t => t.FullName!.Replace('+', '.'));         // İç içe tip adları
        options.SupportNonNullableReferenceTypes();
        options.IgnoreObsoleteActions();
        options.IgnoreObsoleteProperties();

        // XML yorumları varsa ekle (yoksa atla; 500 üretmesin)
        var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
}
