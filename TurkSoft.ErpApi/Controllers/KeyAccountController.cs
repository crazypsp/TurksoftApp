using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeyAccountController : ControllerBase
    {
        private readonly IFirmaService _service;
        public FirmaController(IFirmaService service)
        {

            _service = service;
        }
        //GET: api/firma
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }
        //GET: api/firma/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("Firma Bulunamadı.");
            return Ok(result);
        }
        //POST: api/firma
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Firma firma)
        {
            var result = await _service.AddAsync(firma);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        //PUT: api/firma/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Firma firma)
        {
            if (id != firma.Id)
                return BadRequest("Id uyuşmuyor.");
            var result = await _service.UpdateAsync(firma);
            return Ok(result);
        }
        //DELETE: api/firma/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("Firma bulunamadı veya silinemedi.");
            return NoContent();
        }
    }
}
