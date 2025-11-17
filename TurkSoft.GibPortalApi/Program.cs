using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TurkSoft.Data.GibData;          // ✅ GibAppDbContext
using TurkSoft.Service;               // ✅ AddEntityServices / AddGibEntityServices
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TurkSoft.GibPortalApi v1");
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
