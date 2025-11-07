using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class RolePermission: BaseEntity
    {
        public long Id { get; set; }
        public long RoleId { get; set; }
        public long PermissionId { get; set; }

        // Navigation
        [ValidateNever] public Role Role { get; set; }
        [ValidateNever] public Permission Permission { get; set; }
    }
}
