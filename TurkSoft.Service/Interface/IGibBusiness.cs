using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TurkSoft.Entities.GIBEntityDB;

namespace TurkSoft.Service.Interface
{
    // Ortak sonuç tipi
    public readonly record struct HttpResult<T>(bool Ok, T? Data, string? Error, int StatusCode)
    {
        public static HttpResult<T> Success(T data, int status) => new(true, data, null, status);
        public static HttpResult<T> Fail(string error, int status) => new(false, default, error, status);
    }

    // e-Defter gönderim DTO'su
    public sealed class PostEDefterRequest
    {
        public string StartDate { get; set; } = default!; // yyyy-MM-dd
        public string EndDate { get; set; } = default!;
        public int SplitSize { get; set; } = 50;
        public bool TimeStamp { get; set; }
        public bool WithoutTaxDetail { get; set; }
    }

    // Inbox "IsNew" update DTO
    public sealed class InboxInvoiceIsNewUpdate
    {
        public Guid InvoiceId { get; set; }
        public bool IsNew { get; set; }
    }

    // Ticari e-Fatura cevap DTO
    public sealed class InvoiceResponseRequest
    {
        public Guid InvoiceId { get; set; }
        public int Status { get; set; } // 1:Kabul, 2:Red vs.
        public string Reason { get; set; } = default!;
    }

    // E-Arşiv mail tekrar DTO (POST /v1/earchive/retryinvoicemail)
    public sealed class EArchiveMailRetryRequest
    {
        public Guid Id { get; set; }
        public string EmailAddresses { get; set; } = default!;
    }

    public interface IGibBusiness : IDisposable
    {
        #region Ayarlar / Kullanıcı Bazlı Options

        /// <summary>
        /// İşlem yapan kullanıcıya göre ApiKey, Gönderici VKN ve Inbox Alias bilgilerini
        /// runtime'da günceller. Controller'lar GİB çağrısından önce bunu kullanır.
        /// </summary>
        /// <param name="apiKey">Kullanıcıya ait GİB ApiKey</param>
        /// <param name="senderVkn">GİB tarafında mükellef VKN/TCKN</param>
        /// <param name="inboxAlias">Giden e-fatura için kullanılacak alias</param>
        void UpdateUserOptions(string apiKey, string senderVkn, string inboxAlias);

        #endregion

        #region Kontör
        Task<(int remaining, int used)> GetBalanceAsync(string companyVkn, CancellationToken ct = default);
        Task<(int remaining, int used)> ConsumeBalanceAsync(string companyVkn, int amount, string reason, CancellationToken ct = default);
        #endregion

        #region StaticList
        Task<HttpResult<object>> GetStaticListUnitAsync(CancellationToken ct = default);
        Task<HttpResult<object>> GetStaticListTaxExemptionReasonsAsync(CancellationToken ct = default);
        Task<HttpResult<object>> GetStaticListWithHoldingAsync(CancellationToken ct = default);
        Task<HttpResult<object>> GetStaticListTaxTypeCodeAsync(CancellationToken ct = default);
        Task<HttpResult<object>> GetStaticListTaxOfficeAsync(CancellationToken ct = default);
        Task<HttpResult<object>> GetStaticListCountryAsync(CancellationToken ct = default);
        #endregion

        #region e-Fatura Outbox
        Task<HttpResult<object>> SendEInvoiceJsonAsync(Invoice inv, bool isExport = false, bool consumeKontor = true, string? kontorVkn = null, string? targetAlias = null, CancellationToken ct = default);

        Task<HttpResult<object>> SendEInvoiceUblAsync(Stream fileStream, string fileName,
            int appType, int status, bool useManualInvoiceId,
            string? targetAlias, bool? useFirstAlias, string? prefix,
            string? localReferenceId, bool? checkLocalReferenceId, string? xsltCode,
            bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default);

        Task<HttpResult<object>> UpdateEInvoiceUblAsync(Guid ettn, Stream fileStream, string fileName,
            int appType, int status, bool useManualInvoiceId,
            string? targetAlias, bool? useFirstAlias,
            string? localReferenceId, bool? checkLocalReferenceId, string? xsltCode,
            bool consumeKontor = false, string? kontorVkn = null, CancellationToken ct = default);

        Task<HttpResult<object>> UpdateEInvoiceJsonAsync(Guid ettn, object body, CancellationToken ct = default);
        Task<HttpResult<object>> UpdateEInvoiceStatusAsync(IEnumerable<Guid> ids, int status, CancellationToken ct = default);
        Task<HttpResult<object>> GetEInvoiceOutboxStatusAsync(Guid ettn, CancellationToken ct = default);
        Task<HttpResult<byte[]>> GetEInvoiceOutboxPdfAsync(Guid ettn, bool standardXslt, CancellationToken ct = default);
        Task<HttpResult<byte[]>> GetEInvoiceOutboxHtmlAsync(Guid ettn, bool standardXslt, CancellationToken ct = default);
        Task<HttpResult<byte[]>> GetEInvoiceOutboxUblAsync(Guid ettn, CancellationToken ct = default);
        Task<HttpResult<object>> GetOutboxInvoicesWithNullLocalReferencesAsync(DateTime start, DateTime end, CancellationToken ct = default);
        Task<HttpResult<object>> GetOutboxInvoiceReasonAsync(Guid ettn, CancellationToken ct = default);
        #endregion

        #region e-Fatura Inbox
        Task<HttpResult<object>> GetEInvoiceInboxAsync(DateTime start, DateTime end,
            int pageIndex, int pageSize, bool isNew, CancellationToken ct = default);
        Task<HttpResult<byte[]>> GetEInvoiceInboxHtmlAsync(Guid ettn, bool standardXslt, CancellationToken ct = default);
        Task<HttpResult<byte[]>> GetEInvoiceInboxPdfAsync(Guid ettn, bool standardXslt, CancellationToken ct = default);
        Task<HttpResult<byte[]>> GetEInvoiceInboxUblAsync(Guid ettn, CancellationToken ct = default);
        Task<HttpResult<object>> GetEInvoiceInboxStatusAsync(Guid ettn, CancellationToken ct = default);
        Task<HttpResult<object>> SendInvoiceResponseAsync(InvoiceResponseRequest request, CancellationToken ct = default);
        Task<HttpResult<object>> RetryInvoiceResponseListAsync(IEnumerable<Guid> invoiceIds, CancellationToken ct = default);
        Task<HttpResult<object>> UpdateInboxInvoiceIsNewAsync(IEnumerable<InboxInvoiceIsNewUpdate> items, CancellationToken ct = default);
        #endregion

        #region e-Arşiv
        Task<HttpResult<object>> SendEArchiveJsonAsync(Invoice inv, bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default);
        Task<HttpResult<object>> SendEArchiveJsonRawAsync(object body, bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default);
        Task<HttpResult<object>> SendEArchiveUblAsync(Stream fileStream, string fileName,
            int status, bool useManualInvoiceId, bool sendEmail,
            string? emailAddress, string? prefix, string? localReferenceId,
            bool? checkLocalReferenceId, string? xsltCode, bool? allowOldEArsivCustomer,
            bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default);

        Task<HttpResult<object>> UpdateEArchiveUblAsync(Guid id, Stream fileStream, string fileName,
            int status, bool sendEmail, string emailAddress,
            string? localReferenceId, bool? checkLocalReferenceId, string? xsltCode, bool? allowOldEArsivCustomer,
            CancellationToken ct = default);

        Task<HttpResult<object>> GetEArchiveStatusAsync(Guid ettn, CancellationToken ct = default);
        Task<HttpResult<byte[]>> GetEArchivePdfAsync(Guid ettn, bool standardXslt, CancellationToken ct = default);
        Task<HttpResult<byte[]>> GetEArchiveHtmlAsync(Guid ettn, bool standardXslt, CancellationToken ct = default);
        Task<HttpResult<byte[]>> GetEArchiveUblAsync(Guid ettn, CancellationToken ct = default);
        Task<HttpResult<object>> GetEArchiveWithNullLocalReferencesAsync(DateTime start, DateTime end, CancellationToken ct = default);
        Task<HttpResult<object>> RetryEArchiveMailAsync(Guid id, CancellationToken ct = default);
        Task<HttpResult<object>> RetryEArchiveMailToAddressesAsync(EArchiveMailRetryRequest request, CancellationToken ct = default);
        Task<HttpResult<object>> CancelEArchiveAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        #endregion

        #region e-İrsaliye
        Task<HttpResult<object>> SendDespatchUblAsync(Invoice inv, string? targetAlias = null, bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default);
        Task<HttpResult<object>> UpdateDespatchStatusListAsync(IEnumerable<Guid> ids, int status, CancellationToken ct = default);
        Task<HttpResult<object>> GetDespatchInboxAsync(DateTime start, DateTime end, int pageIndex, int pageSize, bool isDesc, CancellationToken ct = default);
        #endregion

        #region e-Defter
        Task<HttpResult<object>> GetEDefterPeriodListAsync(int pageIndex, int pageSize, bool isDesc, CancellationToken ct = default);
        Task<HttpResult<object>> PostEDefterAsync(PostEDefterRequest model, Stream zipStream, string fileName,
            bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default);
        Task<HttpResult<object>> SendLetterPatentsToGibAsync(CancellationToken ct = default);
        #endregion

        #region GIB User
        Task<HttpResult<byte[]>> GetGibUserRecipientZipAsync(CancellationToken ct = default);
        #endregion
    }
}
