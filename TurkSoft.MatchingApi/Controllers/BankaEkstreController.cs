using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using TurkSoft.Core.Result.Interface;
using TurkSoft.Entities.Document;
using TurkSoft.Entities.Luca;
using TurkSoft.Service.Interface;

namespace TurkSoft.MatchingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankaEkstreController : ControllerBase
    {
        private readonly IBankaEkstreAnalyzerService _service;

        public BankaEkstreController(IBankaEkstreAnalyzerService service)
        {
            _service = service;
        }

        /// <summary>
        /// Banka hareketleri ile muhasebe kodlarını GPT üzerinden eşleştirir.
        /// </summary>
        [HttpPost("eslestir")]
        public async Task<IActionResult> Eslestir([FromBody] BankaEkstreRequestWrapper wrapper)
        {
            var request = wrapper.Request;
            // Servis katmanını çağırıp eşleşme sonucunu alın
            IDataResult<List<LucaFisRow>> sonuc = await _service.HesapKodlariIleEsleAsync(
               request.Hareketler,
               request.HesapKodlari,
               request.KeywordMap,
               request.BankaHesapKodu
           );

            if (!sonuc.Success)
                return BadRequest(sonuc.Message);

            return Ok(sonuc.Data);
        }
    }

    // İstek gövdesi için model
    public class BankaEkstreRequestWrapper
    {
        public BankaEkstreEslestirmeRequest Request { get; set; } = new();
    }
    // Burada zaten aynı namespace içinde, bu yüzden ek using gerekmez:
    public class BankaEkstreEslestirmeRequest
    {
        public List<BankaHareket> Hareketler { get; set; } = new();
        public List<AccountingCode> HesapKodlari { get; set; } = new();
        public Dictionary<string, string> KeywordMap { get; set; } = new();
        public string BankaHesapKodu { get; set; }
    }
}

