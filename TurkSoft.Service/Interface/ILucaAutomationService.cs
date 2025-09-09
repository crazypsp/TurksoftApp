using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Core.Result.Interface;
using TurkSoft.Entities.Luca;

namespace TurkSoft.Service.Interface
{
    /// <summary>
    /// Luca işlemleri için dış dünyaya açılan servis arayüzü.
    /// İş katmanındaki operasyonları kullanır.
    /// </summary>
    public interface ILucaAutomationService
    {
        Task<IResult> LoginAsync(LucaLoginRequest request);
        Task<IDataResult<List<CompanyCode>>> GetCompanyAsync();
        Task<IResult> SelectCompanyAsync(string companyCode);
        Task<IDataResult<List<AccountingCode>>> GetAccountingPlanAsync();
        Task<IResult> SendFisRowsAsync(List<LucaFisRow> rows);
    }
}
