using System;

namespace TurkSoft.Entities.Entities
{
    public class ExportLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ExportType { get; set; } // EXCEL, PDF
        public string FileName { get; set; }
        public string FilterCriteria { get; set; } // JSON formatında
        public int RecordCount { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual User User { get; set; }
    }
}