using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsAyarController : ControllerBase
    {
        private readonly ISmsAyarService _service;
        public SmsAyarController(ISmsAyarService service)
        {
            _service = service;
        }
        //GET: api/smsAyar
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        //GET: api/smsAyar/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("Sms Ayar Bulunamadı.");
            return Ok(result);
        }
        //POST: api/smsAyar
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] SmsAyar smsAyar)
        {
            var result = await _service.AddAsync(smsAyar);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        //PUT: api/smsAyar/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SmsAyar smsAyar)
        {
            if (id != smsAyar.Id)
                return BadRequest("Id uyuşmuyor.");
            var result = await _service.UpdateAsync(smsAyar);
            return Ok(result);
        }
        //DELETE: api/smsAyar/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("Sms Ayar bulunamadı veya silinemedi.");
            return NoContent();
        }
    }
}
