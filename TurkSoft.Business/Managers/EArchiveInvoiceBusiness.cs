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
    public class EArchiveInvoiceBusiness : BaseBusiness, IEArchiveInvoiceBusiness
    {
        private const string ServiceUrl = "https://servis.kolayentegrasyon.net/EArchiveInvoiceService/EArchiveInvoiceWS";

        public EArchiveInvoiceBusiness(string username, string password)
            : base(username, password) { }

        public List<EntResponse> SendInvoice(List<InputDocument> inputDocuments)
        {
            // XML veya JSON içeriği oluşturulabilir
            // PostAsync(ServiceUrl, xmlContent);
            return new List<EntResponse>();
        }

        public List<EntResponse> UpdateInvoice(List<InputDocument> inputDocuments) => new();
        public EntResponse CancelInvoice(string invoiceUuid, string cancelReason, DateTime cancelDate) => new();
        public DocumentQueryResponse QueryInvoice(string paramType, string parameter, string withXML) => new();
        public EntResponse SetEmailSent(List<string> invoiceUuids) => new();
        public CreditInfo GetCustomerCreditCount(string vknTckn) => new();
        public CreditInfo GetCustomerCreditSpace(string vknTckn) => new();
        public EntResponse ControlInvoiceXML(string invoiceXML) => new();
        public DocumentQueryResponse QueryInvoiceWithLocalId(string localId) => new();
        public DocumentQueryResponse QueryInvoiceWithDocumentDate(string start, string end, string withXML, string minRecordId) => new();
        public DocumentQueryResponse QueryInvoiceWithReceivedDate(string start, string end, string withXML, string minRecordId) => new();
        public DocumentQueryResponse QueryInvoicesWithGUIDList(List<string> guidList) => new();
        public DocumentQueryResponse IsEFaturaUser(string vknTckn) => new();
        public DocumentQueryResponse GetCreditActionsForCustomer(string vknTckn) => new();
        public LogResponse GetEAInvoiceStatusWithLogs(List<string> documentUuids) => new();
        public DocumentQueryResponse QueryArchivedInvoice(string paramType, string parameter, string withXML, int fiscalYear) => new();
        public DocumentQueryResponse QueryArchivedInvoiceWithDocumentDate(string start, string end, string withXML, string minRecordId) => new();
        public EntResponse GetDraftDocumentPreview(string xmlContent, string previewType, bool addDraftWatermark) => new();
        public XsltListResponse GetXsltList() => new();
        public PrefixCodeResponse AddPrefixList(string prefixType, string prefixKey) => new();
        public EntResponse UploadXslt(string name, byte[] content, bool isDefault) => new();
        public EntResponse SetDefaultXslt(string xsltName) => new();
        public PrefixCodeResponse SetPrefixStatusList(string prefixType, string prefixKey, bool active) => new();
    }
}
