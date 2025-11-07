using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Request: BaseEntity
    {
        public int Id { get; set; }
        public string Uuid { get; set; }
        public long UserId { get; set; }
        public int SubjectType { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime ResponseDate { get; set; }
        public string Response { get; set; }

        // Navigation
        [ValidateNever] public User User { get; set; }
    }
}
