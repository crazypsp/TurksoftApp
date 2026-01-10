using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.ServiceEntity.Models
{
    public class MatchingLog
    {
        [Key]
        public long Id { get; set; }

        public long BankTransactionId { get; set; }

        public string? Request { get; set; }

        public string? Response { get; set; }

        public bool IsSuccess { get; set; }

        public DateTime LogDate { get; set; } = DateTime.UtcNow;

        public int? UserId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [ForeignKey("BankTransactionId")]
        public virtual BankTransaction? BankTransaction { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
