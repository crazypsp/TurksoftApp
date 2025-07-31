using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.Document;
using TurkSoft.Service.Interface;

namespace TurkSoft.DocumentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankaEkstreController : ControllerBase
    {
        private readonly IBankaEkstreService _service;

        public BankaEkstreController(IBankaEkstreService service)
        {
            _service = service;
        }

        [HttpPost("excel-oku")]
        public async Task<IActionResult> OkuExcel([FromForm] ExcelUploadRequest request)
        {
            if (request.Dosya == null || request.Dosya.Length == 0)
                return BadRequest("Dosya bulunamadı.");

            var result = await _service.OkuExcelAsync(request.Dosya, request.KlasorYolu);
            return Ok(result);
        }

        [HttpPost("pdf-oku")]
        public async Task<IActionResult> OkuPdf([FromForm] ExcelUploadRequest request)
        {
            if (request.Dosya == null || request.Dosya.Length == 0)
                return BadRequest("Dosya bulunamadı.");

            var result = await _service.OkuPdfAsync(request.Dosya, request.KlasorYolu);
            return Ok(result);
        }

        [HttpPost("txt-oku")]
        public async Task<IActionResult> OkuTxt([FromForm] ExcelUploadRequest request)
        {
            if (request.Dosya == null || request.Dosya.Length == 0)
                return BadRequest("Dosya bulunamadı.");

            var result = await _service.OkuTxtAsync(request.Dosya, request.KlasorYolu);
            return Ok(result);
        }

        [HttpPost("txt-yaz")]
        public async Task<IActionResult> YazTxt([FromBody] List<HesapKodEsleme> eslemeler, [FromQuery] string klasorYolu)
        {
            var result = await _service.YazTxtAsync(eslemeler, klasorYolu);
            return Ok(result);
        }
    }
}
