using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.ServiceEntity.Models
{
    public class TransferLog
    {
        [Key]
        public long Id { get; set; }

        public long BankTransactionId { get; set; }

        [StringLength(50)]
        public string? TransactionType { get; set; }

        public string? XmlData { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        public DateTime LogDate { get; set; } = DateTime.UtcNow;

        public int? UserId { get; set; }

        [ForeignKey("BankTransactionId")]
        public virtual BankTransaction? BankTransaction { get; set; }
    }
}
