using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.ServiceEntity.Models
{
    public class BankAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BankId { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? AccountName { get; set; }

        [StringLength(50)]
        public string? Iban { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "TRY";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("BankId")]
        public virtual Bank? Bank { get; set; }
    }
}
