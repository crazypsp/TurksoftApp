using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.EntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsGonderimController : ControllerBase
    {
        private readonly ISmsGonderimService _service;
        public SmsGonderimController(ISmsGonderimService service)
        {
            _service = service;
        }
        //GET: api/smsGonderim
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        //GET: api/smsGonderim/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("sms Gonderim Bulunamadı.");
            return Ok(result);
        }
        //POST: api/smsGonderim
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] SmsGonderim smsGonderim)
        {
            var result = await _service.AddAsync(smsGonderim);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        //PUT: api/smsGonderim/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SmsGonderim smsGonderim)
        {
            if (id != smsGonderim.Id)
                return BadRequest("Id uyuşmuyor.");
            var result = await _service.UpdateAsync(smsGonderim);
            return Ok(result);
        }
        //DELETE: api/smsGonderim/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("sms Gonderim bulunamadı veya silinemedi.");
            return NoContent();
        }
    }
}
