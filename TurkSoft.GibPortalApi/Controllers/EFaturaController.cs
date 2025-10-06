using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TurkSoft.Entities.GIB;
using TurkSoft.Service.Interface;

namespace TurkSoft.GibPortalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EFaturaController : ControllerBase
    {
        private readonly IEFaturaService _service;

        public EFaturaController(IEFaturaService service)
        {
            _service = service;
        }

        [HttpPost("SendUBLInvoice")]
        public async Task<IActionResult> SendUBLInvoice([FromBody] List<InputDocument> invoices)
        {
            var result = await _service.SendUBLInvoiceAsync(invoices);
            return Ok(result);
        }

        [HttpPost("UpdateUBLInvoice")]
        public async Task<IActionResult> UpdateUBLInvoice([FromBody] List<InputDocument> invoices)
        {
            var result = await _service.UpdateUBLInvoiceAsync(invoices);
            return Ok(result);
        }

        [HttpPost("CancelUBLInvoice")]
        public async Task<IActionResult> CancelUBLInvoice(string uuid, string reason, DateTime cancelDate)
        {
            var result = await _service.CancelUBLInvoiceAsync(uuid, reason, cancelDate);
            return Ok(result);
        }

        [HttpGet("QueryUBLInvoice")]
        public async Task<IActionResult> QueryUBLInvoice(string paramType, string parameter, string withXML)
        {
            var result = await _service.QueryUBLInvoiceAsync(paramType, parameter, withXML);
            return Ok(result);
        }

        [HttpGet("GetCustomerCreditCount")]
        public async Task<IActionResult> GetCustomerCreditCount(string vknTckn)
        {
            var result = await _service.GetCustomerCreditCountAsync(vknTckn);
            return Ok(result);
        }

        [HttpPost("ControlUBLXml")]
        public async Task<IActionResult> ControlUBLXml([FromBody] string xmlContent)
        {
            var result = await _service.ControlUBLXmlAsync(xmlContent);
            return Ok(result);
        }
    }
}
