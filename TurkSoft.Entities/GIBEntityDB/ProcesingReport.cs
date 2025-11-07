using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class ProcesingReport: BaseEntity
    {
        public int Id { get; set; }
        public string Uuid { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Period { get; set; }
        public string Section { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public int CompanyId { get; set; }
        public long UserId { get; set; }

        // Navigation
        [ValidateNever] public User User { get; set; }
    }
}
