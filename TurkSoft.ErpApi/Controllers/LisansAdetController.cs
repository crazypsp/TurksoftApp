using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LisansAdetController : ControllerBase
    {
        private readonly ILisansAdetService _service;
        public LisansAdetController(ILisansAdetService service)
        {
            _service = service;
        }
        //GET: api/LisansAdet
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        //GET: api/LisansAdet/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("LisansAdet Bulunamadı.");
            return Ok(result);
        }
        //POST: api/LisansAdet
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] LisansAdet LisansAdet)
        {
            var result = await _service.AddAsync(LisansAdet);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        //PUT: api/LisansAdet/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] LisansAdet LisansAdet)
        {
            if (id != LisansAdet.Id)
                return BadRequest("Id uyuşmuyor.");
            var result = await _service.UpdateAsync(LisansAdet);
            return Ok(result);
        }
        //DELETE: api/LisansAdet/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("LisansAdet bulunamadı veya silinemedi.");
            return NoContent();
        }
    }
}
