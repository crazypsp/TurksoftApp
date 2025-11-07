using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class CompanyInformation: BaseEntity
    {
        [Key]
        public long Id { get; set; }

        [Required, StringLength(11)]
        public string TaxNo { get; set; }               // VKN veya TCKN

        [Required, StringLength(150)]
        public string CompanyName { get; set; }         // Firma / Müşteri Adı

        [StringLength(250)]
        public string Title { get; set; }               // Ticari Unvan

        [StringLength(250)]
        public string KepAddress { get; set; }          // KEP adresi

        [StringLength(11)]
        public string ResponsibleTckn { get; set; }     // Sorumlu kişi TCKN

        [StringLength(100)]
        public string ResponsibleName { get; set; }     // Sorumlu Adı

        [StringLength(100)]
        public string ResponsibleSurname { get; set; }  // Sorumlu Soyadı

        [StringLength(100)]
        public string TaxOffice { get; set; }           // Vergi Dairesi

        [StringLength(200)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string District { get; set; }

        [StringLength(200)]
        public string Website { get; set; }

        [StringLength(200)]
        public string MersisNo { get; set; }

        [StringLength(300)]
        public string LogoUrl { get; set; }
    }
}
