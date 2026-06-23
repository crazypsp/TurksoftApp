using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities.Models;
using TurkSoft.Service.Inferfaces;

namespace TurkSoft.Service.Implementations
{
    /// <summary>
    /// Logo Tiger (UnityObjects COM) kütüphanesi KAYITLI OLMAYAN ortamlar için stub implementasyon.
    /// Bu sınıf yalnızca <c>EnableLogoIntegration != true</c> iken derlenir (bkz. TurkSoft.Service.csproj).
    /// COM kütüphanesi kayıtlı bir makinede <c>-p:EnableLogoIntegration=true</c> ile derlerseniz,
    /// gerçek implementasyon (<c>LogoTigerIntegrationService.cs</c>) devreye girer ve bu dosya derlemeden çıkarılır.
    /// Amaç: Logo'ya bağımlı olmayan projelerin (ErpApi, LucaApi, WebUI vb.) sorunsuz derlenmesini sağlamak.
    /// </summary>
    public class LogoTigerIntegrationService : ILogoTigerIntegrationService
    {
        private const string NotAvailable =
            "Logo Tiger entegrasyonu bu derlemede devre dışı (UnityObjects COM kütüphanesi bulunamadı). " +
            "Etkinleştirmek için TurkSoft.Service projesini -p:EnableLogoIntegration=true ile derleyin.";

        private readonly ILogger<LogoTigerIntegrationService> _logger;

        public LogoTigerIntegrationService(ILogger<LogoTigerIntegrationService> logger)
        {
            _logger = logger;
        }

        public Task<ServiceResult<BankaFisSonuc>> GelenHavaleEkleAsync(GelenHavaleRequest request)
            => throw new NotSupportedException(NotAvailable);

        public Task<ServiceResult<BankaFisSonuc>> GidenHavaleEkleAsync(GidenHavaleRequest request)
            => throw new NotSupportedException(NotAvailable);

        public Task<ServiceResult<BankaFisSonuc>> VirmanEkleAsync(VirmanRequest request)
            => throw new NotSupportedException(NotAvailable);

        public Task<ServiceResult<KrediTaksitSonuc>> KrediTaksitOdemeEkleAsync(KrediTaksitRequest request)
            => throw new NotSupportedException(NotAvailable);

        public Task<ServiceResult<IslemDurumuSonuc>> IslemDurumuKontrolAsync(int fisReferans)
            => throw new NotSupportedException(NotAvailable);
    }
}
