using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WhatsappAyarController : ControllerBase
    {
        private readonly IWhatsappAyarService _service;
        public WhatsappAyarController(IWhatsappAyarService service)
        {
            _service = service;
        }
        //GET: api/whatsappAyar
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        //GET: api/whatsappAyar/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("Whatsapp Ayar Bulunamadı.");
            return Ok(result);
        }
        //POST: api/whatsappAyar
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] WhatsappAyar whatsappAyar)
        {
            var result = await _service.AddAsync(whatsappAyar);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        //PUT: api/whatsappAyar/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] WhatsappAyar whatsappAyar)
        {
            if (id != whatsappAyar.Id)
                return BadRequest("Id uyuşmuyor.");
            var result = await _service.UpdateAsync(whatsappAyar);
            return Ok(result);
        }
        //DELETE: api/whatsappAyar/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("Whatsapp Ayar bulunamadı veya silinemedi.");
            return NoContent();
        }
    }
}
