using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.Document;

namespace TurkSoft.Business.Interface
{
    public interface IBankaEkstreBusiness
    {
        Task<List<BankaHareket>> OkuExcelAsync(IFormFile dosya,string klasorYolu);
        Task<List<BankaHareket>> OkuPDFAsync(IFormFile dosya, string KlasorYolu);
        Task<List<HesapKodEsleme>> OkuTxtAsync(IFormFile dosya,string KlasorYolu);
        Task<bool> YazTxtAsync(List<HesapKodEsleme> eslemeler,string KlasorYolu);
    }
}
