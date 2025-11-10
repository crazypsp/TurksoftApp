using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.GIBEntityDB;
using TurkSoft.Service.Manager;

namespace TurkSoft.Service.Interface
{
    internal interface IGibBusiness
    {
        // ------- Ortak sonuç tipi --------
        public readonly record struct HttpResult<T>(bool Ok, T? Data, string? Error, int StatusCode)
        {
            public static HttpResult<T> Success(T data, int status) => new(true, data, null, status);
            public static HttpResult<T> Fail(string error, int status) => new(false, default, error, status);
        }
        // ------- Interface --------
        public interface IGibBusiness : IDisposable
        {
            // Kontör
            Task<(int remaining, int used)> GetBalanceAsync(string companyVkn, CancellationToken ct = default);
            Task<(int remaining, int used)> ConsumeBalanceAsync(string companyVkn, int amount, string reason, CancellationToken ct = default);

            // e-Fatura / e-İhracat (JSON/UBL)
            Task<HttpResult<object>> SendEInvoiceJsonAsync(Invoice inv, bool isExport = false, bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default);
            Task<HttpResult<object>> SendEInvoiceUblAsync(Stream fileStream, string fileName, bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default);
            Task<HttpResult<object>> UpdateEInvoiceStatusAsync(IEnumerable<Guid> ids, int status, CancellationToken ct = default);
            Task<HttpResult<object>> GetEInvoiceOutboxStatusAsync(Guid ettn, CancellationToken ct = default);
            Task<HttpResult<object>> GetEInvoiceInboxAsync(DateTime start, DateTime end, int pageIndex, int pageSize, bool isDesc, CancellationToken ct = default);
            Task<HttpResult<byte[]>> GetEInvoiceOutboxPdfAsync(Guid ettn, bool standardXslt, CancellationToken ct = default);
            Task<HttpResult<byte[]>> GetEInvoiceOutboxHtmlAsync(Guid ettn, bool standardXslt, CancellationToken ct = default);
            Task<HttpResult<byte[]>> GetEInvoiceOutboxUblAsync(Guid ettn, CancellationToken ct = default);
            Task<HttpResult<byte[]>> GetEInvoiceOutboxZipAsync(Guid ettn, bool standardXslt, CancellationToken ct = default);

            // e-Arşiv
            Task<HttpResult<object>> SendEArchiveJsonAsync(Invoice inv, bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default);
            Task<HttpResult<object>> SendEArchiveUblAsync(Stream fileStream, string fileName, bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default);
            Task<HttpResult<object>> GetEArchiveStatusAsync(Guid ettn, CancellationToken ct = default);
            Task<HttpResult<object>> CancelEArchiveAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

            // e-İrsaliye (UBL JSON: base64 ZIP)
            Task<HttpResult<object>> SendDespatchUblAsync(Invoice inv, string? targetAlias = null, bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default);
            Task<HttpResult<object>> UpdateDespatchStatusListAsync(IEnumerable<Guid> ids, int status, CancellationToken ct = default);
            Task<HttpResult<object>> GetDespatchInboxAsync(DateTime start, DateTime end, int pageIndex, int pageSize, bool isDesc, CancellationToken ct = default);

            // e-Defter
            Task<HttpResult<object>> GetEDefterPeriodListAsync(int pageIndex, int pageSize, bool isDesc, CancellationToken ct = default);
            Task<HttpResult<object>> PostEDefterAsync(PostEDefterRequest model, Stream zipStream, string fileName,
                                          bool consumeKontor = true, string? kontorVkn = null, CancellationToken ct = default);
            Task<HttpResult<object>> SendLetterPatentsToGibAsync(CancellationToken ct = default);
            Task<HttpResult<object>> PostEDefterAsync(GibBusiness.PostEDefterRequest model, Stream zipStream, string fileName, bool consumeKontor, string? kontorVkn, CancellationToken ct);
        }
        // ------- e-Defter gönderim DTO'su (public, interface tarafından kullanılır) --------
        public sealed class PostEDefterRequest
        {
            public string StartDate { get; set; } = default!; // yyyy-MM-dd
            public string EndDate { get; set; } = default!;
            public int SplitSize { get; set; } = 50;
            public bool TimeStamp { get; set; }
            public bool WithoutTaxDetail { get; set; }
        }
    }
}
