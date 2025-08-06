using TurkSoft.Business.Interface;
using TurkSoft.Service.Interface;
using TurkSoft.Service.Manager;

var builder = WebApplication.CreateBuilder(args);
// ? Service ve Business Katmaný Baðlantýsý
builder.Services.AddScoped<IBankaEkstreAnalyzerService, BankaEkstreAnalyzerManagerSrv>();
builder.Services.AddScoped<IBankaEkstreAnalyzerBusiness, BankaEkstreAnalyzerManager>();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
