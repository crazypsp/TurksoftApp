using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class CustomReport: BaseEntity
    {
        public int Id { get; set; }
        public string Uuid { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public int ReportType { get; set; }
        public string SelectedColumns { get; set; } // JSON

        // Navigation
        [ValidateNever] public User User { get; set; }
    }
}
