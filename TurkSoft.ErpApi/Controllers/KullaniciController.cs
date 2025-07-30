using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KullaniciController : ControllerBase
    {
        private readonly IKullaniciService _service;
        public KullaniciController(IKullaniciService service)
        {
            _service = service;
        }
        //GET: api/kullanici
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        //GET: api/kullanici/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("Kullanici Bulunamadı.");
            return Ok(result);
        }
        //POST: api/kullanici
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Kullanici kullanici)
        {
            var result = await _service.AddAsync(kullanici);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        //PUT: api/kullanici/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Kullanici kullanici)
        {
            if (id != kullanici.Id)
                return BadRequest("Id uyuşmuyor.");
            var result = await _service.UpdateAsync(kullanici);
            return Ok(result);
        }
        //DELETE: api/kullanici/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("Kullanici bulunamadı veya silinemedi.");
            return NoContent();
        }
    }
}
