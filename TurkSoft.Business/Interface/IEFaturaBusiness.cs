using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.GIB;

namespace TurkSoft.Business.Interface
{
    public interface IEFaturaBusiness
    {
        List<EntResponse> SendUBLInvoice(List<InputDocument> invoices);
        List<EntResponse> UpdateUBLInvoice(List<InputDocument> invoices);
        EntResponse CancelUBLInvoice(string uuid, string reason, DateTime cancelDate);
        DocumentQueryResponse QueryUBLInvoice(string paramType, string parameter, string withXML);
        CreditInfo GetCustomerCreditCount(string vknTckn);
        EntResponse ControlUBLXml(string xmlContent);
    }
}
