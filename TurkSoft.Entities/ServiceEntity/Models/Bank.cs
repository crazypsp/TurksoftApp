using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.ServiceEntity.Models
{
    public class Bank
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string BankCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string BankName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ApiUrl { get; set; }

        [StringLength(255)]
        public string? ApiKey { get; set; }

        [StringLength(255)]
        public string? ApiSecret { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
