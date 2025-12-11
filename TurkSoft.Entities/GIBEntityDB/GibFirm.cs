using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using TurkSoft.Entities.GIBEntityDB;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class GibFirm : BaseEntity
    {
        public long Id { get; set; }

        // ==== Temel Firma Bilgileri ====

        /// <summary>Firma unvanı</summary>
        public string Title { get; set; }

        /// <summary>VKN / TCKN</summary>
        public string TaxNo { get; set; }

        /// <summary>Vergi dairesi adı</summary>
        public string TaxOffice { get; set; }

        /// <summary>Vergi dairesi ili (ör: İZMİR)</summary>
        public string TaxOfficeProvince { get; set; }

        /// <summary>Ticaret Sicil No</summary>
        public string CommercialRegistrationNo { get; set; }

        /// <summary>Mersis No</summary>
        public string MersisNo { get; set; }

        /// <summary>Müşteri Adı (ekrandaki "Müşteri Adı")</summary>
        public string CustomerName { get; set; }

        /// <summary>Gerçek kişi için Adı (*Tckn)</summary>
        public string PersonalFirstName { get; set; }

        /// <summary>Gerçek kişi için Soyadı (*Tckn)</summary>
        public string PersonalLastName { get; set; }

        /// <summary>Kurum türü (0:KAMU,1:ÖZEL,3:VUK507 ÖZEL,2:VUK507 KAMU)</summary>
        public int InstitutionType { get; set; }

        /// <summary>Müşteri Temsilcisi</summary>
        public string CustomerRepresentative { get; set; }

        // ==== Adres & İletişim ====
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }

        /// <summary>Genel telefon</summary>
        public string Phone { get; set; }

        /// <summary>Genel e-posta</summary>
        public string Email { get; set; }

        /// <summary>Kurumsal e-posta (ekrandaki "Kurumsal e-Posta")</summary>
        public string CorporateEmail { get; set; }

        /// <summary>Kep adresi</summary>
        public string KepAddress { get; set; }

        // ==== Sorumlu kişi bilgileri ====
        public string ResponsibleTckn { get; set; }
        public string ResponsibleFirstName { get; set; }
        public string ResponsibleLastName { get; set; }
        public string ResponsibleMobilePhone { get; set; }
        public string ResponsibleEmail { get; set; }

        // ==== Müşteri kaydını alan kişi ====
        public string CreatedByPersonFirstName { get; set; }
        public string CreatedByPersonLastName { get; set; }
        public string CreatedByPersonMobilePhone { get; set; }

        // ==== GİB ve e-dönüşüm ====

        /// <summary>Genel GİB alias (posta kutusu)</summary>
        public string GibAlias { get; set; }

        /// <summary>Turkcell / ePlatform API key</summary>
        public string ApiKey { get; set; }

        public bool IsEInvoiceRegistered { get; set; }
        public bool IsEArchiveRegistered { get; set; }

        // ==== Navigations ====
        [ValidateNever] public ICollection<Invoice> Invoices { get; set; }
        [ValidateNever] public ICollection<GibUserCreditAccount> CreditAccounts { get; set; }

        /// <summary>Firma bazlı e-dönüşüm hizmetleri (E-Fatura/E-Arşiv, E-İrsaliye, vb.)</summary>
        [ValidateNever] public ICollection<GibFirmService> Services { get; set; }
    }
}
