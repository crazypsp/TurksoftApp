using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailAyarController : ControllerBase
    {
        private readonly IMailAyarService _service;
        public MailAyarController(IMailAyarService service)
        {
            _service = service;
        }
        //GET: api/mailAyar
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        //GET: api/mailAyar/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("MailAyar Bulunamadı.");
            return Ok(result);
        }
        //POST: api/mailAyar
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MailAyar mailAyar)
        {
            var result = await _service.AddAsync(mailAyar);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        //PUT: api/mailAyar/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MailAyar mailAyar)
        {
            if (id != mailAyar.Id)
                return BadRequest("Id uyuşmuyor.");
            var result = await _service.UpdateAsync(mailAyar);
            return Ok(result);
        }
        //DELETE: api/mailAyar/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("MailAyar bulunamadı veya silinemedi.");
            return NoContent();
        }
    }
}
