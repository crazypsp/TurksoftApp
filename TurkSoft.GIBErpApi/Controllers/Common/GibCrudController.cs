using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using TurkSoft.Service.Interface;

namespace TurkSoft.GIBErpApi.Controllers.Common
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public abstract class GibCrudController<T, TService> : ControllerBase
        where T : class
        where TService : IEntityGibService<T, long>
    {
        protected readonly TService _service;
        protected GibCrudController(TService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll(CancellationToken ct = default)
            => Ok(await _service.GetAllAsync(ct));

        [HttpGet("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> GetById(long id, CancellationToken ct = default)
        {
            var entity = await _service.GetByIdAsync(id, ct);
            return entity is null ? NotFound() : Ok(entity);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] T entity)
            => Ok(await _service.AddAsync(entity));

        [HttpPut("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Update(long id, [FromBody] T entity, CancellationToken ct = default)
        {
            // Entity üzerinde Id property’sini bul
            var idProp = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            if (idProp is null)
                return BadRequest("Id alanı bulunamadı.");

            // Route’tan gelen id ile entity.Id’yi senkronla
            var idType = Nullable.GetUnderlyingType(idProp.PropertyType) ?? idProp.PropertyType;
            var convertedId = Convert.ChangeType(id, idType);

            var current = idProp.GetValue(entity);
            if (current == null || !current.Equals(convertedId))
            {
                idProp.SetValue(entity, convertedId);
            }

            var updated = await _service.UpdateAsync(entity, ct);
            return Ok(updated);
        }


        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> Delete(long id, CancellationToken ct = default)
            => (await _service.DeleteAsync(id, ct)) ? NoContent() : NotFound($"{typeof(T).Name} silinemedi veya bulunamadı.");
    }
}
