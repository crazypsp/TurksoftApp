// ERPAPI/Controllers/Custom/MaliMusavirController.Attach.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TurkSoft.Data.Context;          // AppDbContext
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.ErpApi.Controllers
{
    // NOT: Bu sınıf partial; burada [ApiController], [Route], ctor YOK.
    public partial class MaliMusavirController
    {
        public sealed class RefIdDto { public Guid Id { get; set; } } // body: { "Id": "<entityId>" }

        // MM'ye KeyAccount ekle: POST /api/v1/malimusavir/{mmId}/keyaccounts
        [HttpPost("{mmId:guid}/keyaccounts")]
        public async Task<IActionResult> AttachKeyAccount(
            Guid mmId,
            [FromBody] RefIdDto dto,
            [FromServices] AppDbContext db,
            CancellationToken ct = default)
        {
            // 1) MM var mı?
            var mm = await db.Set<MaliMusavir>().FindAsync(new object[] { mmId }, ct);
            if (mm == null) return NotFound("Mali müşavir bulunamadı.");

            // 2) KeyAccount'u, MM koleksiyonu dahil yükle
            var ka = await db.Set<KeyAccount>()
                             .Include(x => x.MaliMusavirs)
                             .FirstOrDefaultAsync(x => x.Id == dto.Id, ct);
            if (ka == null) return NotFound("KeyAccount bulunamadı.");

            // 3) Zaten ekli değilse ekle
            if (!ka.MaliMusavirs.Any(x => x.Id == mmId))
                ka.MaliMusavirs.Add(mm);

            await db.SaveChangesAsync(ct);
            return NoContent(); // 204
        }

        // MM'ye Luca ekle: POST /api/v1/malimusavir/{mmId}/luca
        [HttpPost("{mmId:guid}/luca")]
        public async Task<IActionResult> AttachLuca(
            Guid mmId,
            [FromBody] RefIdDto dto,
            [FromServices] AppDbContext db,
            CancellationToken ct = default)
        {
            var mm = await db.Set<MaliMusavir>().FindAsync(new object[] { mmId }, ct);
            if (mm == null) return NotFound("Mali müşavir bulunamadı.");

            var luca = await db.Set<Luca>()
                               .Include(x => x.MaliMusavirs)
                               .FirstOrDefaultAsync(x => x.Id == dto.Id, ct);
            if (luca == null) return NotFound("Luca kaydı bulunamadı.");

            if (!luca.MaliMusavirs.Any(x => x.Id == mmId))
                luca.MaliMusavirs.Add(mm);

            await db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
