using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.Document;

namespace TurkSoft.Service.Interface
{
    public interface IBankaEkstreService
    {
        Task<List<BankaHareket>> OkuExcelAsync(IFormFile dosya, string klasorYolu);
        Task<List<BankaHareket>> OkuPdfAsync(IFormFile dosya, string klasorYolu);
        Task<List<HesapKodEsleme>> OkuTxtAsync(IFormFile dosya, string klasorYolu);
        Task<bool> YazTxtAsync(List<HesapKodEsleme> eslemeler, string klasorYolu);
    }
}
