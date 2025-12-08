using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class InvoiceDesignTemplate : BaseEntity
    {
        public long Id { get; set; }

        /// <summary>Şablon adı (örn: Standart, Detaylı)</summary>
        public string Name { get; set; }

        /// <summary>Açıklama (opsiyonel)</summary>
        public string Description { get; set; }

        /// <summary>Belge tipi (Fatura, İrsaliye vb) için DocumentType FK</summary>
        public long? DocumentTypeId { get; set; }

        /// <summary>Şablon tipi (XSLT, HTML, Razor vs.)</summary>
        public string TemplateType { get; set; }

        /// <summary>Şablon içeriği (XSLT / HTML / JSON)</summary>
        public string Content { get; set; }

        /// <summary>Varsayılan şablon mu?</summary>
        public bool IsDefault { get; set; }

        public DocumentType DocumentType { get; set; }
    }
}
