using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class BranchInformation: BaseEntity
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("CompanyInformation")]
        public long CompanyId { get; set; }

        public string Type { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        [ValidateNever] public CompanyInformation CompanyInformation { get; set; }
    }
}
