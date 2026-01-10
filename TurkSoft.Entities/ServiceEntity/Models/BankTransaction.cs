using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.ServiceEntity.Models
{
    public class BankTransaction
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public int BankAccountId { get; set; }

        public DateTime TransactionDate { get; set; }

        public DateTime? ProcessDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Balance { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        public int TransactionType { get; set; }

        public int Status { get; set; } = 0; // 0:Beklemede, 1:Eşleştirildi, 2:Aktarıldı

        [StringLength(50)]
        public string? SenderAccount { get; set; }

        [StringLength(50)]
        public string? ReceiverAccount { get; set; }

        [StringLength(100)]
        public string? SenderName { get; set; }

        [StringLength(100)]
        public string? ReceiverName { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? MatchedDate { get; set; }

        public DateTime? TransferredDate { get; set; }

        [StringLength(50)]
        public string? MatchedAccountCode { get; set; }

        [StringLength(50)]
        public string? BankProcType { get; set; }

        [Column(TypeName = "xml")]
        public string? SourceData { get; set; }

        [ForeignKey("BankAccountId")]
        public virtual BankAccount? BankAccount { get; set; }
    }
}
