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
    public class EArchiveInvoiceService : IEArchiveInvoiceService
    {
        private readonly IEArchiveInvoiceBusiness _business;

        public EArchiveInvoiceService(IEArchiveInvoiceBusiness business)
        {
            _business = business;
        }

        public Task<List<EntResponse>> SendInvoiceAsync(List<InputDocument> inputDocuments)
            => Task.Run(() => _business.SendInvoice(inputDocuments));

        public Task<List<EntResponse>> UpdateInvoiceAsync(List<InputDocument> inputDocuments)
            => Task.Run(() => _business.UpdateInvoice(inputDocuments));

        public Task<EntResponse> CancelInvoiceAsync(string invoiceUuid, string cancelReason, DateTime cancelDate)
            => Task.Run(() => _business.CancelInvoice(invoiceUuid, cancelReason, cancelDate));

        public Task<DocumentQueryResponse> QueryInvoiceAsync(string paramType, string parameter, string withXML)
            => Task.Run(() => _business.QueryInvoice(paramType, parameter, withXML));

        public Task<EntResponse> SetEmailSentAsync(List<string> invoiceUuidList)
            => Task.Run(() => _business.SetEmailSent(invoiceUuidList));

        public Task<CreditInfo> GetCustomerCreditCountAsync(string vknTckn)
            => Task.Run(() => _business.GetCustomerCreditCount(vknTckn));

        public Task<CreditInfo> GetCustomerCreditSpaceAsync(string vknTckn)
            => Task.Run(() => _business.GetCustomerCreditSpace(vknTckn));

        public Task<EntResponse> ControlInvoiceXMLAsync(string invoiceXML)
            => Task.Run(() => _business.ControlInvoiceXML(invoiceXML));

        public Task<DocumentQueryResponse> QueryInvoiceWithLocalIdAsync(string localId)
            => Task.Run(() => _business.QueryInvoiceWithLocalId(localId));

        public Task<DocumentQueryResponse> QueryInvoiceWithDocumentDateAsync(string startDate, string endDate, string withXML, string minRecordId)
            => Task.Run(() => _business.QueryInvoiceWithDocumentDate(startDate, endDate, withXML, minRecordId));

        public Task<DocumentQueryResponse> QueryInvoiceWithReceivedDateAsync(string startDate, string endDate, string withXML, string minRecordId)
            => Task.Run(() => _business.QueryInvoiceWithReceivedDate(startDate, endDate, withXML, minRecordId));

        public Task<DocumentQueryResponse> QueryInvoicesWithGUIDListAsync(List<string> guidList)
            => Task.Run(() => _business.QueryInvoicesWithGUIDList(guidList));

        public Task<DocumentQueryResponse> IsEFaturaUserAsync(string vknTckn)
            => Task.Run(() => _business.IsEFaturaUser(vknTckn));

        public Task<DocumentQueryResponse> GetCreditActionsForCustomerAsync(string vknTckn)
            => Task.Run(() => _business.GetCreditActionsForCustomer(vknTckn));

        public Task<LogResponse> GetEAInvoiceStatusWithLogsAsync(List<string> documentUuids)
            => Task.Run(() => _business.GetEAInvoiceStatusWithLogs(documentUuids));

        public Task<DocumentQueryResponse> QueryArchivedInvoiceAsync(string paramType, string parameter, string withXML, int fiscalYear)
            => Task.Run(() => _business.QueryArchivedInvoice(paramType, parameter, withXML, fiscalYear));

        public Task<DocumentQueryResponse> QueryArchivedInvoiceWithDocumentDateAsync(string startDate, string endDate, string withXML, string minRecordId)
            => Task.Run(() => _business.QueryArchivedInvoiceWithDocumentDate(startDate, endDate, withXML, minRecordId));

        public Task<EntResponse> GetDraftDocumentPreviewAsync(string xmlContent, string previewType, bool addDraftWatermark)
            => Task.Run(() => _business.GetDraftDocumentPreview(xmlContent, previewType, addDraftWatermark));

        public Task<XsltListResponse> GetXsltListAsync()
            => Task.Run(() => _business.GetXsltList());

        public Task<PrefixCodeResponse> AddPrefixListAsync(string prefixType, string prefixKey)
            => Task.Run(() => _business.AddPrefixList(prefixType, prefixKey));

        public Task<EntResponse> UploadXsltAsync(string name, byte[] content, bool isDefault)
            => Task.Run(() => _business.UploadXslt(name, content, isDefault));

        public Task<EntResponse> SetDefaultXsltAsync(string xsltName)
            => Task.Run(() => _business.SetDefaultXslt(xsltName));

        public Task<PrefixCodeResponse> SetPrefixStatusListAsync(string prefixType, string prefixKey, bool active)
            => Task.Run(() => _business.SetPrefixStatusList(prefixType, prefixKey, active));
    }
}
