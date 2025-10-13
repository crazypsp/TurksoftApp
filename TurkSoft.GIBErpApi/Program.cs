using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using TurkSoft.Data.GibData;
using TurkSoft.GIBErpApi.Infrastructure.Auth;
using TurkSoft.Service;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// SERVICES
// =====================================================

// ---- GIB DbContext (Sadece GIB tablolarý için)
builder.Services.AddDbContext<GibAppDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    )
);

// ---- Ýþ Katmaný Servisleri (Sadece GIB)
builder.Services.AddGibEntityServices();

// ---- API Versioning
builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
    opt.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// ---- Versioned API Explorer (Swagger için gerekli)
builder.Services.AddVersionedApiExplorer(opt =>
{
    opt.GroupNameFormat = "'v'VVV";
    opt.SubstituteApiVersionInUrl = true;
});

// ---- Controllers + JSON options
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// ---- CORS
var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Default", p =>
    {
        p.WithOrigins(allowed)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials();
    });
});

// ---- Swagger (Versiyonlu)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TurkSoft.GibErpApi",
        Version = "v1",
        Description = "GIB veritabaný için ERP API (JWT + Refresh + Cookie destekli)"
    });

    // XML yorumlarýný Swagger’a dahil et
    var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    options.CustomSchemaIds(t => t.FullName!.Replace('+', '.'));

    // Swagger’da JWT Auth (Bearer)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ---- Proxy/Forwarded headers
builder.Services.Configure<ForwardedHeadersOptions>(opt =>
{
    opt.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// ---- Form Limitleri
builder.Services.Configure<FormOptions>(o =>
{
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = long.MaxValue;
    o.MultipartHeadersLengthLimit = int.MaxValue;
});

// =====================================================
// AUTHENTICATION + AUTHORIZATION
// =====================================================

// JWT Ayarlarýný oku
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwtOpt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpt.SigningKey));

// Dinamik kimlik doðrulama: Cookie veya Bearer
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = "Dynamic";
        options.DefaultChallengeScheme = "Dynamic";
    })
    .AddPolicyScheme("Dynamic", "Dynamic (Cookie or Bearer)", opts =>
    {
        opts.ForwardDefaultSelector = ctx =>
        {
            var auth = ctx.Request.Headers["Authorization"].ToString();
            return auth?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
                ? JwtBearerDefaults.AuthenticationScheme
                : CookieAuthenticationDefaults.AuthenticationScheme;
        };
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opts =>
    {
        opts.Cookie.Name = "gib_auth";
        opts.LoginPath = "/api/v1/auth/login-web";
        opts.LogoutPath = "/api/v1/auth/logout-web";
        opts.SlidingExpiration = true;
        opts.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOpt.Issuer,
            ValidAudience = jwtOpt.Audience,
            IssuerSigningKey = jwtKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// ---- Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
    options.AddPolicy("CanManageInvoices", p =>
        p.RequireAssertion(ctx =>
            ctx.User.IsInRole("Admin") || ctx.User.IsInRole("Finance")));
});

// ---- Infra servisleri (DI)
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddHttpContextAccessor();

// =====================================================
// PIPELINE
// =====================================================

var app = builder.Build();

// ---- Swagger Aktifse
if (app.Configuration.GetSection("Swagger:Enabled").Get<bool>())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var desc in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", $"GIB API {desc.GroupName}");
        }
        options.RoutePrefix = app.Configuration["Swagger:RoutePrefix"] ?? "swagger";
    });
}

app.UseForwardedHeaders();
app.UseCors("Default");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
