using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UrunTipiController : ControllerBase
    {
        private readonly IUrunTipiService _service;
        public UrunTipiController(IUrunTipiService service)
        {
            _service = service;
        }
        //GET: api/urunTipi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        //GET: api/urunTipi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("Urun Tipi Bulunamadı.");
            return Ok(result);
        }
        //POST: api/urunTipi
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] UrunTipi urunTipi)
        {
            var result = await _service.AddAsync(urunTipi);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        //PUT: api/urunTipi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UrunTipi urunTipi)
        {
            if (id != urunTipi.Id)
                return BadRequest("Id uyuşmuyor.");
            var result = await _service.UpdateAsync(urunTipi);
            return Ok(result);
        }
        //DELETE: api/urunTipi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("Urun Tipi bulunamadı veya silinemedi.");
            return NoContent();
        }
    }
}
