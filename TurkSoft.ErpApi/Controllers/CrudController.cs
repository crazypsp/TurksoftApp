// Controllers/Common/CrudController.cs (GÜNCEL)

using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using TurkSoft.Service.Interface;

namespace TurkSoft.ErpApi.Controllers.Common
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public abstract class CrudController<T, TService> : ControllerBase
        where T : class
        where TService : IEntityService<T>
    {
        protected readonly TService _service;
        protected CrudController(TService service) => _service = service;

        // GET all
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll(CancellationToken ct = default)
            => Ok(await _service.GetAllAsync(ct));

        // GET by id
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        {
            var res = await _service.GetByIdAsync(id, ct);
            return res is null ? NotFound($"{typeof(T).Name} bulunamadı.") : Ok(res);
        }

        // POST
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Add([FromBody] T entity, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var created = await _service.AddAsync(entity, ct);

            var idProp = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            if (idProp?.GetValue(created) is Guid gid)
                return CreatedAtAction(nameof(GetById), new { id = gid, version = "1.0" }, created);

            return Created(string.Empty, created);
        }

        // PUT
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Update(Guid id, [FromBody] T entity, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var idProp = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            if (idProp?.GetValue(entity) is Guid eid && eid != id)
                return BadRequest("Id uyuşmuyor.");

            return Ok(await _service.UpdateAsync(entity, ct));
        }

        // DELETE (soft)
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
            => (await _service.DeleteAsync(id, ct)) ? NoContent() : NotFound($"{typeof(T).Name} bulunamadı veya silinemedi.");
    }
}
