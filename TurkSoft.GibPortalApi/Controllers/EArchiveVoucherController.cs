using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.GIB;
using TurkSoft.Service.Interface;

namespace TurkSoft.GibPortalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EArchiveVoucherController : ControllerBase
    {
        private readonly IEArchiveVoucherService _service;

        public EArchiveVoucherController(IEArchiveVoucherService service)
        {
            _service = service;
        }

        // 1️⃣ SMM Gönderme
        [HttpPost("SendSMM")]
        public async Task<IActionResult> SendSMM([FromBody] List<InputDocument> vouchers)
            => Ok(await _service.SendSMMAsync(vouchers));

        // 2️⃣ MM Gönderme
        [HttpPost("SendMM")]
        public async Task<IActionResult> SendMM([FromBody] List<InputDocument> vouchers)
            => Ok(await _service.SendMMAsync(vouchers));

        // 3️⃣ SMM Güncelleme
        [HttpPost("UpdateSMM")]
        public async Task<IActionResult> UpdateSMM([FromBody] List<InputDocument> vouchers)
            => Ok(await _service.UpdateSMMAsync(vouchers));

        // 4️⃣ MM Güncelleme
        [HttpPost("UpdateMM")]
        public async Task<IActionResult> UpdateMM([FromBody] List<InputDocument> vouchers)
            => Ok(await _service.UpdateMMAsync(vouchers));

        // 5️⃣ SMM İptali
        [HttpPost("CancelSMM")]
        public async Task<IActionResult> CancelSMM(string voucherUuid, string cancelReason, DateTime cancelDate)
            => Ok(await _service.CancelSMMAsync(voucherUuid, cancelReason, cancelDate));

        // 6️⃣ MM İptali
        [HttpPost("CancelMM")]
        public async Task<IActionResult> CancelMM(string voucherUuid, string cancelReason, DateTime cancelDate)
            => Ok(await _service.CancelMMAsync(voucherUuid, cancelReason, cancelDate));

        // 7️⃣ Son SMM Bilgisi
        [HttpGet("GetLastSMMIdAndDate")]
        public async Task<IActionResult> GetLastSMMIdAndDate(string sourceId, [FromBody] List<string> documentIdPrefixList)
            => Ok(await _service.GetLastSMMIdAndDateAsync(sourceId, documentIdPrefixList));

        // 8️⃣ Son MM Bilgisi
        [HttpGet("GetLastMMIdAndDate")]
        public async Task<IActionResult> GetLastMMIdAndDate(string sourceId, [FromBody] List<string> documentIdPrefixList)
            => Ok(await _service.GetLastMMIdAndDateAsync(sourceId, documentIdPrefixList));

        // 9️⃣ Makbuz Sorgulama
        [HttpGet("QueryVouchers")]
        public async Task<IActionResult> QueryVouchers(string paramType, string parameter, string voucherType, string withXML)
            => Ok(await _service.QueryVouchersAsync(paramType, parameter, voucherType, withXML));

        // 🔟 SMM E-Posta İşaretleme
        [HttpPost("SetSmmEmailSent")]
        public async Task<IActionResult> SetSmmEmailSent([FromBody] List<string> uuids)
            => Ok(await _service.SetSmmEmailSentAsync(uuids));

        // 11️⃣ MM E-Posta İşaretleme
        [HttpPost("SetMmEmailSent")]
        public async Task<IActionResult> SetMmEmailSent([FromBody] List<string> uuids)
            => Ok(await _service.SetMmEmailSentAsync(uuids));

        // 12️⃣ Kontör Sorgulama
        [HttpGet("GetCustomerCreditCount")]
        public async Task<IActionResult> GetCustomerCreditCount(string vknTckn)
            => Ok(await _service.GetCustomerCreditCountAsync(vknTckn));

        // 13️⃣ SMM Belge İşaretleme
        [HttpPost("SetSmmDocumentFlag")]
        public async Task<IActionResult> SetSmmDocumentFlag([FromBody] FlagSetter flagSetter)
            => Ok(await _service.SetSmmDocumentFlagAsync(flagSetter));

        // 14️⃣ MM Belge İşaretleme
        [HttpPost("SetMmDocumentFlag")]
        public async Task<IActionResult> SetMmDocumentFlag([FromBody] FlagSetter flagSetter)
            => Ok(await _service.SetMmDocumentFlagAsync(flagSetter));

        // 15️⃣ SMM XML Kontrolü
        [HttpPost("ControlXmlSmm")]
        public async Task<IActionResult> ControlXmlSmm([FromBody] string xml)
            => Ok(await _service.ControlXmlSmmAsync(xml));

        // 16️⃣ MM XML Kontrolü
        [HttpPost("ControlXmlMm")]
        public async Task<IActionResult> ControlXmlMm([FromBody] string xml)
            => Ok(await _service.ControlXmlMmAsync(xml));

        // 17️⃣ LocalId ile Sorgulama
        [HttpGet("QueryVouchersWithLocalId")]
        public async Task<IActionResult> QueryVouchersWithLocalId(string localId, string voucherType)
            => Ok(await _service.QueryVouchersWithLocalIdAsync(localId, voucherType));

        // 18️⃣ Belge Tarihine Göre Sorgu
        [HttpGet("QueryVouchersWithDocumentDate")]
        public async Task<IActionResult> QueryVouchersWithDocumentDate(string startDate, string endDate, string voucherType, string withXML, string minRecordId)
            => Ok(await _service.QueryVouchersWithDocumentDateAsync(startDate, endDate, voucherType, withXML, minRecordId));

        // 19️⃣ Alınma Tarihine Göre Sorgu
        [HttpGet("QueryVouchersWithReceivedDate")]
        public async Task<IActionResult> QueryVouchersWithReceivedDate(string startDate, string endDate, string voucherType, string withXML, string minRecordId)
            => Ok(await _service.QueryVouchersWithReceivedDateAsync(startDate, endDate, voucherType, withXML, minRecordId));

        // 20️⃣ UUID Listesi ile Sorgu
        [HttpPost("QueryVouchersWithGUIDList")]
        public async Task<IActionResult> QueryVouchersWithGUIDList([FromBody] List<string> guidList, string voucherType)
            => Ok(await _service.QueryVouchersWithGUIDListAsync(guidList, voucherType));

        // 21️⃣ E-Fatura Kullanıcısı mı?
        [HttpGet("IsEFaturaUser")]
        public async Task<IActionResult> IsEFaturaUser(string vknTckn)
            => Ok(await _service.IsEFaturaUserAsync(vknTckn));

        // 22️⃣ Kredi Hareketleri
        [HttpGet("GetCreditActionsForCustomer")]
        public async Task<IActionResult> GetCreditActionsForCustomer(string vknTckn)
            => Ok(await _service.GetCreditActionsForCustomerAsync(vknTckn));
    }
}
