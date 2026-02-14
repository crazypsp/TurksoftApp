// Program.cs
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TurkSoft.BankWebUI.Services;
using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers;
using TurkSoft.Data.EntityData; // TurkSoftDbContext burada
using TurkSoft.Service;
using TurkSoft.Service.Implementations;
using TurkSoft.Service.Inferfaces;
using TurkSoft.Service.Interface;
using TurkSoft.Service.Manager;
using TurkSoft.Services;
using TurkSoft.Services.Implementations;
using TurkSoft.Services.Interfaces;
using TurkSoft.Services.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

// Database Context - TurkSoftDbContext kullan
builder.Services.AddDbContext<TurkSoftDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Service Registrations
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBankService, BankService>();
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<IBankCredentialService, BankCredentialService>();
builder.Services.AddScoped<IBankTransactionService, BankTransactionService>();
builder.Services.AddScoped<IExportLogService, ExportLogService>();
builder.Services.AddScoped<IMatchingLogService, MatchingLogService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ISystemLogService, SystemLogService>();
builder.Services.AddScoped<ITransactionImportService, TransactionImportService>();
builder.Services.AddScoped<ITransferLogService, TransferLogService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IBankaEkstreAnalyzerService, BankaEkstreAnalyzerManagerSrv>();
builder.Services.AddScoped<IBankaEkstreAnalyzerBusiness, BankaEkstreAnalyzerManager>(); ;
builder.Services.AddTurkSoftServices();
// Demo servis (mevcut)
builder.Services.AddScoped<IDemoDataService, DemoDataService>();

// ========== TIGER VERTABANI SERVSLER ==========

// Tiger Repository ve Service kaytlar
builder.Services.AddScoped<IClCardRepository, ClCardRepository>();
builder.Services.AddScoped<IClCardService, ClCardService>();
builder.Services.AddScoped<ITigerBankAccountRepository, TigerBankAccountRepository>();
builder.Services.AddScoped<ITigerBankAccountService, TigerBankAccountService>();
builder.Services.AddScoped<ITigerGlAccountRepository, TigerGlAccountRepository>();
builder.Services.AddScoped<ITigerGlAccountService, TigerGlAccountService>();
//tiger ile ilgili dier repository ve service kaytlarn buraya ekleyebilirsiniz
builder.Services.AddScoped<ILogoTigerIntegrationService, LogoTigerIntegrationService>();
// Dier Tiger servislerini de buraya ekleyebilirsiniz
// builder.Services.AddScoped<IDigerTigerService, DigerTigerService>();

// Tiger veritaban iin connection string'i Configuration'dan al
var tigerConnectionString = builder.Configuration.GetConnectionString("TigerConnection");

// Tiger veritaban iin Dapper veya ADO.NET kullanacaksanz:
builder.Services.AddSingleton(new TigerDbConnection(tigerConnectionString));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

// Tiger veritaban iin connection string wrapper snf
public class TigerDbConnection
{
    public string ConnectionString { get; }

    public TigerDbConnection(string connectionString)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
}