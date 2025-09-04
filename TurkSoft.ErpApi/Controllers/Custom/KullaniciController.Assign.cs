using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using TurkSoft.Service.Interface;
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.ErpApi.Controllers
{
    public partial class KullaniciController
    {
        // body: { "TargetId": "...", "IsPrimary": true, "AtananRol": "Bayi" }
        [HttpPost("{userId:guid}/assign/bayi")]
        public async Task<IActionResult> AssignToBayi(
            Guid userId,
            [FromBody] AssignDto dto,
            [FromServices] IKullaniciBayiService svc,
            CancellationToken ct = default)
        {
            var pivot = new KullaniciBayi
            {
                Id = Guid.NewGuid(),
                KullaniciId = userId,
                BayiId = dto.TargetId,
                IsPrimary = dto.IsPrimary,
                AtananRol = dto.AtananRol,
                CreateDate = DateTime.UtcNow,
                IsActive = true
            };
            await svc.AddAsync(pivot, ct);
            return Ok(new { ok = true });
        }

        // body: { "TargetId": "...", "IsPrimary": true, "AtananRol": "MM" }
        [HttpPost("{userId:guid}/assign/malimusavir")]
        public async Task<IActionResult> AssignToMaliMusavir(
            Guid userId,
            [FromBody] AssignDto dto,
            [FromServices] IKullaniciMaliMusavirService svc,
            CancellationToken ct = default)
        {
            var pivot = new KullaniciMaliMusavir
            {
                Id = Guid.NewGuid(),
                KullaniciId = userId,
                MaliMusavirId = dto.TargetId,
                IsPrimary = dto.IsPrimary,
                AtananRol = dto.AtananRol,
                CreateDate = DateTime.UtcNow,
                IsActive = true
            };
            await svc.AddAsync(pivot, ct);
            return Ok(new { ok = true });
        }

        public sealed class AssignDto
        {
            public Guid TargetId { get; set; }
            public bool IsPrimary { get; set; } = true;
            public string? AtananRol { get; set; }
        }
    }
}
