using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    /// <summary>Hizmet bazlı GİB Alias listesi</summary>
    public class GibFirmServiceAlias : BaseEntity
    {
        public long Id { get; set; }

        public long GibFirmServiceId { get; set; }
        [ValidateNever] public GibFirmService Service { get; set; }

        /// <summary>Gönderici / Alıcı yönü ("SENDER" / "RECEIVER")</summary>
        public string Direction { get; set; }

        /// <summary>Alias değeri</summary>
        public string Alias { get; set; }
    }
}
