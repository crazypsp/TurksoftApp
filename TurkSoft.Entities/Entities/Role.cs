using System.Collections.Generic;
using TurkSoft.Entities.GIBEntityDB;

namespace TurkSoft.Entities.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSystemRole { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}