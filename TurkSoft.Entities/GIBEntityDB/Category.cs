using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Category
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public ICollection<ItemsCategory> ItemsCategories { get; set; }
    }
}
