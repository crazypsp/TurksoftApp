using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities.Models;
using TurkSoft.Service.Implementations;

namespace TurkSoft.Service.Inferfaces
{
    public interface ILogoTigerIntegrationService
    {
        Task<ServiceResult<BankaFisSonuc>> GelenHavaleEkleAsync(GelenHavaleRequest request);
        Task<ServiceResult<BankaFisSonuc>> GidenHavaleEkleAsync(GidenHavaleRequest request);
        Task<ServiceResult<BankaFisSonuc>> VirmanEkleAsync(VirmanRequest request);
        Task<ServiceResult<KrediTaksitSonuc>> KrediTaksitOdemeEkleAsync(KrediTaksitRequest request);
        Task<ServiceResult<IslemDurumuSonuc>> IslemDurumuKontrolAsync(int fisReferans);
    }
}
