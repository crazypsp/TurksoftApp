using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Business.Base;
using TurkSoft.Business.Interface;
using TurkSoft.Entities.GIB;

namespace TurkSoft.Business.Managers
{
    public class EFaturaBusiness : BaseBusiness, IEFaturaBusiness
    {
        private const string ServiceUrl = "https://servis.kolayentegrasyon.net/InvoiceService/InvoiceWS";

        public EFaturaBusiness(string username, string password)
            : base(username, password) { }

        public List<EntResponse> SendUBLInvoice(List<InputDocument> invoices) => new();
        public List<EntResponse> UpdateUBLInvoice(List<InputDocument> invoices) => new();
        public EntResponse CancelUBLInvoice(string uuid, string reason, DateTime cancelDate) => new();
        public DocumentQueryResponse QueryUBLInvoice(string paramType, string parameter, string withXML) => new();
        public CreditInfo GetCustomerCreditCount(string vknTckn) => new();
        public EntResponse ControlUBLXml(string xmlContent) => new();
    }
}
