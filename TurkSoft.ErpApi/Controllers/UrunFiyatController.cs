using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UrunFiyatController : ControllerBase
    {
        private readonly IUrunFiyatService _service;
        public UrunFiyatController(IUrunFiyatService service)
        {
            _service = service;
        }
        //GET: api/urunFiyat
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        //GET: api/urunFiyat/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("Urun Fiyat Bulunamadı.");
            return Ok(result);
        }
        //POST: api/urunFiyat
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] UrunFiyat urunFiyat)
        {
            var result = await _service.AddAsync(urunFiyat);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        //PUT: api/urunFiyat/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UrunFiyat urunFiyat)
        {
            if (id != urunFiyat.Id)
                return BadRequest("Id uyuşmuyor.");
            var result = await _service.UpdateAsync(urunFiyat);
            return Ok(result);
        }
        //DELETE: api/urunFiyat/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("Urun Fiyat bulunamadı veya silinemedi.");
            return NoContent();
        }
    }
}
