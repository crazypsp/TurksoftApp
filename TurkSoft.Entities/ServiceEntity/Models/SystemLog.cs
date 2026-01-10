using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.ServiceEntity.Models
{
    public class SystemLog
    {
        [Key]
        public long Id { get; set; }

        public DateTime LogDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? LogLevel { get; set; }

        public string? Message { get; set; }

        public string? Exception { get; set; }

        public int? UserId { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(200)]
        public string? Action { get; set; }

        [StringLength(100)]
        public string? Module { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
