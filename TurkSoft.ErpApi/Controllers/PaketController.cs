using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaketController : ControllerBase
    {
        private readonly IPaketService _service;
        public PaketController(IPaketService service)
        {
            _service = service;
        }
        //GET: api/paket
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        //GET: api/paket/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("paket Bulunamadı.");
            return Ok(result);
        }
        //POST: api/paket
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Paket paket)
        {
            var result = await _service.AddAsync(paket);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        //PUT: api/paket/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Paket paket)
        {
            if (id != paket.Id)
                return BadRequest("Id uyuşmuyor.");
            var result = await _service.UpdateAsync(paket);
            return Ok(result);
        }
        //DELETE: api/paket/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("paket bulunamadı veya silinemedi.");
            return NoContent();
        }
    }
}
