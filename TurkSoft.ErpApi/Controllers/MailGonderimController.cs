using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailGonderimController : ControllerBase
    {
        private readonly IMailGonderimService _service;
        public MailGonderimController(IMailGonderimService service)
        {
            _service = service;
        }
        //GET: api/mailGonderim
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        //GET: api/mailGonderim/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("Mail Gonderim Bulunamadı.");
            return Ok(result);
        }
        //POST: api/mailGonderim
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MailGonderim mailGonderim)
        {
            var result = await _service.AddAsync(mailGonderim);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        //PUT: api/mailGonderim/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MailGonderim mailGonderim)
        {
            if (id != mailGonderim.Id)
                return BadRequest("Id uyuşmuyor.");
            var result = await _service.UpdateAsync(mailGonderim);
            return Ok(result);
        }
        //DELETE: api/mailGonderim/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("Mail Gonderim bulunamadı veya silinemedi.");
            return NoContent();
        }
    }
}
