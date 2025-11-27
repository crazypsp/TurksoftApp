using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TurkSoft.Entities.GIBEntityDB
{
    public enum GibCreditTransactionType
    {
        Load = 1,   // Kontör yükleme
        Usage = 2,   // Fatura gönderiminde kullanılan
        Refund = 3    // İade / iptal
    }

    /// <summary>
    /// Kontör hesabı hareketi (yükleme, kullanım, iade).
    /// </summary>
    public class GibUserCreditTransaction : BaseEntity
    {
        public long Id { get; set; }

        public long GibUserCreditAccountId { get; set; }

        /// <summary>Hareket bir faturaya bağlıysa, ilgili InvoiceId (örn. gönderimde 1 kontör düş)</summary>
        public long? InvoiceId { get; set; }

        public GibCreditTransactionType TransactionType { get; set; }

        /// <summary>Kontör adedi (1 = 1 e-fatura gibi)</summary>
        public int Quantity { get; set; }

        public string Description { get; set; }

        // Navigation
        [ValidateNever] public GibUserCreditAccount GibUserCreditAccount { get; set; }
        [ValidateNever] public Invoice Invoice { get; set; }
    }
}
