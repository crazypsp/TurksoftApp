using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.GIB;

namespace TurkSoft.Service.Interface
{
    public interface IEFaturaService
    {
        Task<List<EntResponse>> SendUBLInvoiceAsync(List<InputDocument> invoices);
        Task<List<EntResponse>> UpdateUBLInvoiceAsync(List<InputDocument> invoices);
        Task<EntResponse> CancelUBLInvoiceAsync(string uuid, string reason, DateTime cancelDate);
        Task<DocumentQueryResponse> QueryUBLInvoiceAsync(string paramType, string parameter, string withXML);
        Task<CreditInfo> GetCustomerCreditCountAsync(string vknTckn);
        Task<EntResponse> ControlUBLXmlAsync(string xmlContent);
    }
}
