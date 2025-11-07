using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Role: BaseEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }

        // Navigation
        [ValidateNever] public ICollection<UserRole> UserRoles { get; set; }
        [ValidateNever] public ICollection<RolePermission> RolePermissions { get; set; }
    }
}
