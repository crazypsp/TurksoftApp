using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.GIB;
using TurkSoft.Service.Interface;

namespace TurkSoft.GibPortalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EArchiveInvoiceController : ControllerBase
    {
        private readonly IEArchiveInvoiceService _service;

        public EArchiveInvoiceController(IEArchiveInvoiceService service)
        {
            _service = service;
        }

        // 1️⃣ Fatura Gönderimi
        [HttpPost("SendInvoice")]
        public async Task<IActionResult> SendInvoice([FromBody] List<InputDocument> documents)
            => Ok(await _service.SendInvoiceAsync(documents));

        // 2️⃣ Fatura Güncelleme
        [HttpPost("UpdateInvoice")]
        public async Task<IActionResult> UpdateInvoice([FromBody] List<InputDocument> documents)
            => Ok(await _service.UpdateInvoiceAsync(documents));

        // 3️⃣ Fatura İptali
        [HttpPost("CancelInvoice")]
        public async Task<IActionResult> CancelInvoice(string invoiceUuid, string cancelReason, DateTime cancelDate)
            => Ok(await _service.CancelInvoiceAsync(invoiceUuid, cancelReason, cancelDate));

        // 4️⃣ Fatura Sorgulama
        [HttpGet("QueryInvoice")]
        public async Task<IActionResult> QueryInvoice(string paramType, string parameter, string withXML)
            => Ok(await _service.QueryInvoiceAsync(paramType, parameter, withXML));

        // 5️⃣ E-posta Gönderildi İşaretleme
        [HttpPost("SetEmailSent")]
        public async Task<IActionResult> SetEmailSent([FromBody] List<string> invoiceUuidList)
            => Ok(await _service.SetEmailSentAsync(invoiceUuidList));

        // 6️⃣ Müşteri Kontör Bilgisi
        [HttpGet("GetCustomerCreditCount")]
        public async Task<IActionResult> GetCustomerCreditCount(string vknTckn)
            => Ok(await _service.GetCustomerCreditCountAsync(vknTckn));

        // 7️⃣ Müşteri Alan Bilgisi
        [HttpGet("GetCustomerCreditSpace")]
        public async Task<IActionResult> GetCustomerCreditSpace(string vknTckn)
            => Ok(await _service.GetCustomerCreditSpaceAsync(vknTckn));

        // 8️⃣ XML Kontrolü
        [HttpPost("ControlInvoiceXML")]
        public async Task<IActionResult> ControlInvoiceXML([FromBody] string invoiceXML)
            => Ok(await _service.ControlInvoiceXMLAsync(invoiceXML));

        // 9️⃣ LocalId ile Sorgulama
        [HttpGet("QueryInvoiceWithLocalId")]
        public async Task<IActionResult> QueryInvoiceWithLocalId(string localId)
            => Ok(await _service.QueryInvoiceWithLocalIdAsync(localId));

        // 🔟 Belge Tarihine Göre Sorgulama
        [HttpGet("QueryInvoiceWithDocumentDate")]
        public async Task<IActionResult> QueryInvoiceWithDocumentDate(string startDate, string endDate, string withXML, string minRecordId)
            => Ok(await _service.QueryInvoiceWithDocumentDateAsync(startDate, endDate, withXML, minRecordId));

        // 11️⃣ Alınma Tarihine Göre Sorgulama
        [HttpGet("QueryInvoiceWithReceivedDate")]
        public async Task<IActionResult> QueryInvoiceWithReceivedDate(string startDate, string endDate, string withXML, string minRecordId)
            => Ok(await _service.QueryInvoiceWithReceivedDateAsync(startDate, endDate, withXML, minRecordId));

        // 12️⃣ UUID Listesi ile Sorgulama
        [HttpPost("QueryInvoicesWithGUIDList")]
        public async Task<IActionResult> QueryInvoicesWithGUIDList([FromBody] List<string> guidList)
            => Ok(await _service.QueryInvoicesWithGUIDListAsync(guidList));

        // 13️⃣ E-Fatura Kullanıcısı mı?
        [HttpGet("IsEFaturaUser")]
        public async Task<IActionResult> IsEFaturaUser(string vknTckn)
            => Ok(await _service.IsEFaturaUserAsync(vknTckn));

        // 14️⃣ Müşteri Kredi Hareketleri
        [HttpGet("GetCreditActionsForCustomer")]
        public async Task<IActionResult> GetCreditActionsForCustomer(string vknTckn)
            => Ok(await _service.GetCreditActionsForCustomerAsync(vknTckn));

        // 15️⃣ İşlem Logları
        [HttpPost("GetEAInvoiceStatusWithLogs")]
        public async Task<IActionResult> GetEAInvoiceStatusWithLogs([FromBody] List<string> documentUuids)
            => Ok(await _service.GetEAInvoiceStatusWithLogsAsync(documentUuids));

        // 16️⃣ Arşivlenmiş Fatura Sorgulama
        [HttpGet("QueryArchivedInvoice")]
        public async Task<IActionResult> QueryArchivedInvoice(string paramType, string parameter, string withXML, int fiscalYear)
            => Ok(await _service.QueryArchivedInvoiceAsync(paramType, parameter, withXML, fiscalYear));

        // 17️⃣ Belge Tarihine Göre Arşiv Sorgulama
        [HttpGet("QueryArchivedInvoiceWithDocumentDate")]
        public async Task<IActionResult> QueryArchivedInvoiceWithDocumentDate(string startDate, string endDate, string withXML, string minRecordId)
            => Ok(await _service.QueryArchivedInvoiceWithDocumentDateAsync(startDate, endDate, withXML, minRecordId));

        // 18️⃣ Taslak Belge Önizleme
        [HttpPost("GetDraftDocumentPreview")]
        public async Task<IActionResult> GetDraftDocumentPreview(string xmlContent, string previewType, bool addDraftWatermark)
            => Ok(await _service.GetDraftDocumentPreviewAsync(xmlContent, previewType, addDraftWatermark));

        // 19️⃣ XSLT Listesi
        [HttpGet("GetXsltList")]
        public async Task<IActionResult> GetXsltList()
            => Ok(await _service.GetXsltListAsync());

        // 20️⃣ Belge Ön Eki Ekleme
        [HttpPost("AddPrefixList")]
        public async Task<IActionResult> AddPrefixList(string prefixType, string prefixKey)
            => Ok(await _service.AddPrefixListAsync(prefixType, prefixKey));

        // 21️⃣ XSLT Yükleme
        [HttpPost("UploadXslt")]
        public async Task<IActionResult> UploadXslt(string name, [FromBody] byte[] content, bool isDefault)
            => Ok(await _service.UploadXsltAsync(name, content, isDefault));

        // 22️⃣ Varsayılan XSLT Belirleme
        [HttpPost("SetDefaultXslt")]
        public async Task<IActionResult> SetDefaultXslt(string xsltName)
            => Ok(await _service.SetDefaultXsltAsync(xsltName));

        // 23️⃣ Belge Ön Eki Durum Güncelleme
        [HttpPost("SetPrefixStatusList")]
        public async Task<IActionResult> SetPrefixStatusList(string prefixType, string prefixKey, bool active)
            => Ok(await _service.SetPrefixStatusListAsync(prefixType, prefixKey, active));
    }
}
