using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class ApiRefreshToken: BaseEntity
    {
        public long Id { get; set; }      
        public string Token { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string? CreatedByIp { get; set; }
        public string? UserAgent { get; set; }

        public DateTime? RevokedAtUtc { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
    }
}
