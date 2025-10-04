using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Business.Interface;
using TurkSoft.Entities.GIB;
using TurkSoft.Service.Interface;

namespace TurkSoft.Service.Manager
{
    public class EFaturaService:IEFaturaService
    {
        private readonly IEFaturaBusiness _business;

        public EFaturaService(IEFaturaBusiness business)
        {
            _business = business;
        }

        public async Task<List<EntResponse>> SendUBLInvoiceAsync(List<InputDocument> invoices)
        {
            return await Task.Run(() => _business.SendUBLInvoice(invoices));
        }

        public async Task<List<EntResponse>> UpdateUBLInvoiceAsync(List<InputDocument> invoices)
        {
            return await Task.Run(() => _business.UpdateUBLInvoice(invoices));
        }

        public async Task<EntResponse> CancelUBLInvoiceAsync(string uuid, string reason, DateTime cancelDate)
        {
            return await Task.Run(() => _business.CancelUBLInvoice(uuid, reason, cancelDate));
        }

        public async Task<DocumentQueryResponse> QueryUBLInvoiceAsync(string paramType, string parameter, string withXML)
        {
            return await Task.Run(() => _business.QueryUBLInvoice(paramType, parameter, withXML));
        }

        public async Task<CreditInfo> GetCustomerCreditCountAsync(string vknTckn)
        {
            return await Task.Run(() => _business.GetCustomerCreditCount(vknTckn));
        }

        public async Task<EntResponse> ControlUBLXmlAsync(string xmlContent)
        {
            return await Task.Run(() => _business.ControlUBLXml(xmlContent));
        }
    }
}
