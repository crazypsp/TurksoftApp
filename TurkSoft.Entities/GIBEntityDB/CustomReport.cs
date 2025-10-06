using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class CustomReport
    {
        public int Id { get; set; }
        public string Uuid { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public long UserId { get; set; }
        public int ReportType { get; set; }
        public string SelectedColumns { get; set; } // JSON
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public User User { get; set; }
    }
}
