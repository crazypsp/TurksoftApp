using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class GibInvoiceOperationLog
    {
        public int Id { get; set; }

        // FK -> Invoice
        public long InvoiceId { get; set; }
        public Invoice Invoice { get; set; }

        // Dönen id (Turkcell/GİB tarafındaki fatura GUID'i)
        public string ExternalInvoiceId { get; set; }  // "id" alanı

        // Dönen fatura numarası
        public string InvoiceNumber { get; set; }      // "invoiceNumber" alanı

        // Hangi API işlemi (ör: "SendInvoice", "GetStatus", "CancelInvoice" vs.)
        public string OperationName { get; set; }

        // Başarılı / Başarısız
        public bool IsSuccess { get; set; }

        // Hata bilgileri (varsa)
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        // İstersen tüm response’u JSON olarak da saklayabilirsin
        public string RawResponseJson { get; set; }

        // İşlemi yapan kullanıcı (opsiyonel)
        public long? UserId { get; set; }
        [ValidateNever] public User User { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
