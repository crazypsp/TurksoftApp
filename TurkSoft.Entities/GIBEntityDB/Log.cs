using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Log
    {
        public long Id { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public long? UserId { get; set; }
        [ValidateNever] public User User { get; set; }
    }
}
