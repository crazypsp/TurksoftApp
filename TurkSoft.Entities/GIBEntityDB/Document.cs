using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Document: BaseEntity
    {
        public long Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long DocumentTypeId { get; set; }

        // Navigation
        [ValidateNever] public DocumentType DocumentType { get; set; }
    }
}
