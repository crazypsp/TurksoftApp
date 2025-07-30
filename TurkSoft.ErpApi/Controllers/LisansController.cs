using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LisansController : ControllerBase
    {
        private readonly ILisansService _service;
        public LisansController(ILisansService service)
        {
            _service = service;
        }
        //GET: api/lisans
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        //GET: api/lisans/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("Lisans Bulunamadı.");
            return Ok(result);
        }
        //POST: api/lisans
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Lisans lisans)
        {
            var result = await _service.AddAsync(lisans);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        //PUT: api/lisans/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Lisans lisans)
        {
            if (id != lisans.Id)
                return BadRequest("Id uyuşmuyor.");
            var result = await _service.UpdateAsync(lisans);
            return Ok(result);
        }
        //DELETE: api/lisans/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("Lisans bulunamadı veya silinemedi.");
            return NoContent();
        }
    }
}
