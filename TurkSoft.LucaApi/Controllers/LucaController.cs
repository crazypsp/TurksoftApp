using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// Service katmanından bağımlılık alınır
        /// </summary>
        public LucaController(ILucaAutomationService lucaService)
        {
            _lucaService = lucaService;
        }

        /// <summary>
        /// Luca sistemine giriş işlemi
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LucaLoginRequest request)
        {
            var result = await _lucaService.LoginAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Luca’dan hesap planını getirir
        /// </summary>
        [HttpGet("hesap-plani")]
        public async Task<IActionResult> GetAccountingPlan()
        {
            var result = await _lucaService.GetAccountingPlanAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Luca’ya fiş satırlarını gönderir
        /// </summary>
        [HttpPost("fis-gonder")]
        public async Task<IActionResult> SendFis([FromBody] List<LucaFisRow> rows)
        {
            var result = await _lucaService.SendFisRowsAsync(rows);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
