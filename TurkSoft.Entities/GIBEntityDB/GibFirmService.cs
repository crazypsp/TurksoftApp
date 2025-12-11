using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    /// <summary>GİB Firma hizmeti – E-Fatura, E-İrsaliye vb.</summary>
    public class GibFirmService : BaseEntity
    {
        public long Id { get; set; }

        public long GibFirmId { get; set; }
        [ValidateNever] public GibFirm GibFirm { get; set; }

        /// <summary>
        /// Hizmet tipi:
        /// "EFATURA_EARSIV", "EIRSALIYE", "EDEFTER", "EMM", "EBILET" vb.
        /// İstersen enum da kullanabilirsin.
        /// </summary>
        public string ServiceType { get; set; }

        /// <summary>Hizmet başlangıç tarihi</summary>
        public DateTime? StartDate { get; set; }

        /// <summary>Hizmet bitiş tarihi (varsa)</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>Tarife türü (ör: STANDART, ÖZEL vb.)</summary>
        public string TariffType { get; set; }

        /// <summary>Durum: AKTIF / PASIF vs. BaseEntity.IsActive de var.</summary>
        public string Status { get; set; }

        [ValidateNever] public ICollection<GibFirmServiceAlias> Aliases { get; set; }
    }
}
