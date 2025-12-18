using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers;
using TurkSoft.Business.Managers.BankProviders;

namespace TurkSoft.Business.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBankingBusiness(this IServiceCollection services)
        {
            services.AddScoped<IBankStatementManager, BankStatementManager>();

            services.AddScoped<IBankStatementProvider, IsBankStatementProvider>();
            services.AddScoped<IBankStatementProvider, AkbankStatementProvider>();
            services.AddScoped<IBankStatementProvider, AlbarakaStatementProvider>();
            services.AddScoped<IBankStatementProvider, KuveytTurkStatementProvider>();
            services.AddScoped<IBankStatementProvider, VakifBankStatementProvider>();

            services.AddMemoryCache();
            services.AddHttpClient();

            return services;
        }
    }
}
