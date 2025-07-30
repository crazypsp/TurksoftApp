using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WhatsappGonderimController : ControllerBase
    {
        private readonly IWhatsappGonderimService _service;
        public WhatsappGonderimController(IWhatsappGonderimService service)
        {
            _service = service;
        }
        //GET: api/whatsappGonderim
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        //GET: api/whatsappGonderim/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("Whatsapp Gonderim Bulunamadı.");
            return Ok(result);
        }
        //POST: api/whatsappGonderim
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] WhatsappGonderim whatsappGonderim)
        {
            var result = await _service.AddAsync(whatsappGonderim);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        //PUT: api/whatsappGonderim/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] WhatsappGonderim whatsappGonderim)
        {
            if (id != whatsappGonderim.Id)
                return BadRequest("Id uyuşmuyor.");
            var result = await _service.UpdateAsync(whatsappGonderim);
            return Ok(result);
        }
        //DELETE: api/whatsappGonderim/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("Whatsapp Gonderim bulunamadı veya silinemedi.");
            return NoContent();
        }
    }
}
