using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Permission: BaseEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        // Navigation
        [ValidateNever] public ICollection<RolePermission> RolePermissions { get; set; }
    }
}
