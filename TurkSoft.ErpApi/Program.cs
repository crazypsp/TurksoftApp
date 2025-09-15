// Program.cs (.NET 8, Asp.Versioning 8.1 uyumlu)

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

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    )
);

builder.Services.AddEntityServices();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        o.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IConfigureOptions<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>,
                              ConfigureSwaggerOptions>();

builder.Services.AddHealthChecks()
    .AddCheck<TurkSoft.ErpApi.HealthChecks.DbContextHealthCheck>("sql");

// CORS (✅ Bu alan kritik!)
builder.Services.AddCors(o =>
{
    o.AddPolicy("WebUICors", p =>
    {
        p.WithOrigins("https://noxmusavir.com")  // ❗ allowedOrigins yerine direkt sabit yazıldı
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials(); // Cookie/Session çalışacaksa şart
    });
});

builder.Services.AddProblemDetails();

builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = long.MaxValue);

builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// -------------------- PIPELINE --------------------

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseForwardedHeaders();

if (app.Environment.IsProduction())
    app.UseHsts();

app.UseHttpsRedirection();

// ✅ CORS middleware burada çalışmalı (HEMEN REDIRECT ARKASINDAN)
app.UseCors("WebUICors");

app.UseWhen(ctx =>
    !ctx.Request.Path.StartsWithSegments("/swagger") &&
    !ctx.Request.Path.StartsWithSegments("/health"),
    branch =>
    {
        branch.UseMiddleware<TurkSoft.ErpApi.Middleware.GlobalExceptionMiddleware>();
    });

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

// 🔐 Eğer auth varsa burayı aç
// app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/", () =>
    Results.Redirect("/" + (builder.Configuration["Swagger:RoutePrefix"] ?? "swagger"))
);

app.Run();

// -------------------- Swagger Options --------------------

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
