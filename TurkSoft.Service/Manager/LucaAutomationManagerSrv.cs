using TurkSoft.Business.Interface;
using TurkSoft.Core.Result.Interface;
using TurkSoft.Entities.Document;
using TurkSoft.Entities.Luca;
using TurkSoft.Service.Interface;

namespace TurkSoft.Service.Manager
{
    /// <summary>
    /// Luca işlemleri için servis yöneticisi.
    /// İş katmanındaki metotları doğrudan çağırır.
    /// </summary>
    public class LucaAutomationManagerSrv : ILucaAutomationService
    {
        private readonly ILucaAutomationBussiness _lucaBusiness;

        // İş katmanı bağımlılığı DI ile alınır
        public LucaAutomationManagerSrv(ILucaAutomationBussiness lucaBusiness)
        {
            _lucaBusiness = lucaBusiness;
        }

        // Login işlemini delegeler
        public async Task<IResult> LoginAsync(LucaLoginRequest request)
        {
            return await _lucaBusiness.LoginAsync(request);
        }

        // Firma Listesi getirir
        public async Task<IDataResult<List<CompanyCode>>> GetCompanyAsync()
        {
            return await _lucaBusiness.GetCompanyAsync();
        }
        //Firma Seçimi yap
        public Task<IResult> SelectCompanyAsync(string companyCode)
            => _lucaBusiness.SelectCompanyAsync(companyCode);

        // Hesap planını getirir
        public async Task<IDataResult<List<AccountingCode>>> GetAccountingPlanAsync()
        {
            return await _lucaBusiness.GetAccountingPlanAsync();
        }

        // Fiş satırlarını gönderir
        public async Task<IResult> SendFisRowsAsync(List<LucaFisRow> rows)
        {
            return await _lucaBusiness.SendFisRowsAsync(rows);
        }
    }
}
