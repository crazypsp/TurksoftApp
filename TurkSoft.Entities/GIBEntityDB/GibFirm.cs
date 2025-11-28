using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using TurkSoft.Entities.GIBEntityDB;

public class GibFirm : BaseEntity
{
    public long Id { get; set; }

    public string Title { get; set; }
    public string TaxNo { get; set; }              // VKN / TCKN
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

    /// 🔹 Turkcell/ePlatform API key
    public string ApiKey { get; set; }

    public bool IsEInvoiceRegistered { get; set; }
    public bool IsEArchiveRegistered { get; set; }

    // Navigation
    [ValidateNever] public ICollection<Invoice> Invoices { get; set; }
    [ValidateNever] public ICollection<GibUserCreditAccount> CreditAccounts { get; set; }
}
