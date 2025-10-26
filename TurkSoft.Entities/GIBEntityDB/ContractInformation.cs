using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class ContractInformation
    {
        [Key]
        public long Id { get; set; }

        [Required, StringLength(11)]
        public string TaxNo { get; set; }

        [Required, StringLength(150)]
        public string CompanyName { get; set; }

        [StringLength(250)]
        public string Title { get; set; }

        [StringLength(250)]
        public string KepAddress { get; set; }

        [StringLength(11)]
        public string ResponsibleTckn { get; set; }

        [StringLength(100)]
        public string ResponsibleName { get; set; }

        [StringLength(100)]
        public string ResponsibleSurname { get; set; }

        [ValidateNever] public ICollection<ContractInformation> Contracts { get; set; }
        [ValidateNever] public ICollection<BranchInformation> Branches { get; set; }

        [ValidateNever] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ValidateNever] public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
