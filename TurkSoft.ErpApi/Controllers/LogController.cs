using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ILogService _service;
        public LogController(ILogService service)
        {
            _service = service;
        }
        //GET: api/log
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        //GET: api/log/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("Log Bulunamadı.");
            return Ok(result);
        }
        //POST: api/log
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Log log)
        {
            var result = await _service.AddAsync(log);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        //PUT: api/log/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Log log)
        {
            if (id != log.Id)
                return BadRequest("Id uyuşmuyor.");
            var result = await _service.UpdateAsync(log);
            return Ok(result);
        }
        //DELETE: api/log/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("Log bulunamadı veya silinemedi.");
            return NoContent();
        }
    }
}
