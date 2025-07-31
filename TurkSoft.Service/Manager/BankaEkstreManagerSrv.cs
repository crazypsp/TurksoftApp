using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Business.Interface;
using TurkSoft.Entities.Document;
using TurkSoft.Service.Interface;

namespace TurkSoft.Service.Manager
{
    public class BankaEkstreManagerSrv:IBankaEkstreService
    {
        private readonly IBankaEkstreBusiness _business;

        public BankaEkstreManagerSrv(IBankaEkstreBusiness business)
        {
            _business = business;
        }

        public Task<List<BankaHareket>> OkuExcelAsync(IFormFile dosya, string klasorYolu)
        {
            return _business.OkuExcelAsync(dosya, klasorYolu);
        }

        public Task<List<BankaHareket>> OkuPdfAsync(IFormFile dosya, string klasorYolu)
        {
            return _business.OkuPDFAsync(dosya, klasorYolu);
        }

        public Task<List<HesapKodEsleme>> OkuTxtAsync(IFormFile dosya, string klasorYolu)
        {
            return _business.OkuTxtAsync(dosya,klasorYolu);
        }

        public Task<bool> YazTxtAsync(List<HesapKodEsleme> eslemeler, string klasorYolu)
        {
            return _business.YazTxtAsync(eslemeler, klasorYolu);
        }
    }
}
