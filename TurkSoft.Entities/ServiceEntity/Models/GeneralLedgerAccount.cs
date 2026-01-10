using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.ServiceEntity.Models
{
    public class GeneralLedgerAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string AccountName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? AccountType { get; set; }

        [StringLength(50)]
        public string? GroupCode { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [StringLength(20)]
        public string? CompanyCode { get; set; }
    }
}
