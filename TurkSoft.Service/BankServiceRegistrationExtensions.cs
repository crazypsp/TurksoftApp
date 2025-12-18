using Microsoft.Extensions.DependencyInjection;
using TurkSoft.Service.Interface;
using TurkSoft.Service.Manager;
using TurkSoft.Business.DependencyInjection;

namespace TurkSoft.Service;

public static class BankServiceRegistrationExtensions
{
    public static IServiceCollection AddTurkSoftServices(this IServiceCollection services)
    {
        services.AddBankingBusiness(); // Business DI
        services.AddScoped<IBankStatementService, BankStatementService>();
        return services;
    }
}
