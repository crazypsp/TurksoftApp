using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace TurkSoft.Entities.GIBEntityDB
{
    /// <summary>
    /// Kullanıcının GİB tarafında fatura gönderdiği kendi firması (mükellef).
    /// </summary>
    public class GibFirm : BaseEntity
    {
        public long Id { get; set; }

        /// <summary>Firma unvanı</summary>
        public string Title { get; set; }

        /// <summary>VKN veya TCKN</summary>
        public string TaxNo { get; set; }

        public string TaxOffice { get; set; }

        public string CommercialRegistrationNo { get; set; }
        public string MersisNo { get; set; }

        public string AddressLine { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }

        /// <summary>GİB alias (posta kutusu)</summary>
        public string GibAlias { get; set; }

        public bool IsEInvoiceRegistered { get; set; }
        public bool IsEArchiveRegistered { get; set; }

        // Navigation
        [ValidateNever] public ICollection<Invoice> Invoices { get; set; }
        [ValidateNever] public ICollection<GibUserCreditAccount> CreditAccounts { get; set; }
    }
}
