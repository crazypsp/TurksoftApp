using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Identifiers: BaseEntity
    {
        public short Id { get; set; }
        public string Uuid { get; set; }
        public string Desc { get; set; }
        public string Value { get; set; }
        public short Type { get; set; }
        public long ItemId { get; set; }

        // Navigation
        [ValidateNever] public Item Item { get; set; }
    }
}
