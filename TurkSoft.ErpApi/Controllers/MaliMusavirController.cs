using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaliMusavirController : ControllerBase
    {
        private readonly IMaliMusavirService _service;
        public MaliMusavirController(IMaliMusavirService service)
        {
            _service = service;
        }
        //GET: api/maliMusavir
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        //GET: api/maliMusavir/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("Mali Musavir Bulunamadı.");
            return Ok(result);
        }
        //POST: api/maliMusavir
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MaliMusavir maliMusavir)
        {
            var result = await _service.AddAsync(maliMusavir);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        //PUT: api/maliMusavir/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MaliMusavir maliMusavir)
        {
            if (id != maliMusavir.Id)
                return BadRequest("Id uyuşmuyor.");
            var result = await _service.UpdateAsync(maliMusavir);
            return Ok(result);
        }
        //DELETE: api/maliMusavir/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("Mali Musavir bulunamadı veya silinemedi.");
            return NoContent();
        }
    }
}
