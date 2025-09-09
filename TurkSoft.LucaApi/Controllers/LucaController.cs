using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.Luca;
using TurkSoft.Service.Interface;

namespace TurkSoft.LucaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LucaController : ControllerBase
    {
        private readonly ILucaAutomationService _lucaService;

        public LucaController(ILucaAutomationService lucaService)
        {
            _lucaService = lucaService;
        }

        /// <summary> Luca sistemine giriş işlemi </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LucaLoginRequest request)
        {
            var result = await _lucaService.LoginAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary> Luca’dan Cari/Şirket Listesini getirir (alias: companies) </summary>
        [HttpGet("cari-list")]
        public async Task<IActionResult> GetCompany()
        {
            var result = await _lucaService.GetCompanyAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("companies")]
        public Task<IActionResult> Companies() => GetCompany();

        /// <summary> Şirketi aktif eder (seçer) – Seçim sonrası SirName/Tamam otomatik tıklanır </summary>
        public record SelectCompanyDto(string Code);

        [HttpPost("select-company")]
        public async Task<IActionResult> SelectCompany([FromBody] SelectCompanyDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest(new { message = "Şirket kodu zorunludur." });

            var result = await _lucaService.SelectCompanyAsync(dto.Code);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary> Luca’dan hesap planını getirir </summary>
        [HttpGet("hesap-plani")]
        public async Task<IActionResult> GetAccountingPlan()
        {
            var result = await _lucaService.GetAccountingPlanAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary> Luca’ya fiş satırlarını gönderir </summary>
        [HttpPost("fis-gonder")]
        public async Task<IActionResult> SendFis([FromBody] List<LucaFisRow> rows)
        {
            var result = await _lucaService.SendFisRowsAsync(rows);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
