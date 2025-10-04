using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.GIB;

namespace TurkSoft.Service.Interface
{
    public interface IEArchiveInvoiceService
    {
        Task<List<EntResponse>> SendInvoiceAsync(List<InputDocument> inputDocuments);
        Task<List<EntResponse>> UpdateInvoiceAsync(List<InputDocument> inputDocuments);
        Task<EntResponse> CancelInvoiceAsync(string invoiceUuid, string cancelReason, DateTime cancelDate);
        Task<DocumentQueryResponse> QueryInvoiceAsync(string paramType, string parameter, string withXML);
        Task<EntResponse> SetEmailSentAsync(List<string> invoiceUuidList);
        Task<CreditInfo> GetCustomerCreditCountAsync(string vknTckn);
        Task<CreditInfo> GetCustomerCreditSpaceAsync(string vknTckn);
        Task<EntResponse> ControlInvoiceXMLAsync(string invoiceXML);
        Task<DocumentQueryResponse> QueryInvoiceWithLocalIdAsync(string localId);
        Task<DocumentQueryResponse> QueryInvoiceWithDocumentDateAsync(string startDate, string endDate, string withXML, string minRecordId);
        Task<DocumentQueryResponse> QueryInvoiceWithReceivedDateAsync(string startDate, string endDate, string withXML, string minRecordId);
        Task<DocumentQueryResponse> QueryInvoicesWithGUIDListAsync(List<string> guidList);
        Task<DocumentQueryResponse> IsEFaturaUserAsync(string vknTckn);
        Task<DocumentQueryResponse> GetCreditActionsForCustomerAsync(string vknTckn);
        Task<LogResponse> GetEAInvoiceStatusWithLogsAsync(List<string> documentUuids);
        Task<DocumentQueryResponse> QueryArchivedInvoiceAsync(string paramType, string parameter, string withXML, int fiscalYear);
        Task<DocumentQueryResponse> QueryArchivedInvoiceWithDocumentDateAsync(string startDate, string endDate, string withXML, string minRecordId);
        Task<EntResponse> GetDraftDocumentPreviewAsync(string xmlContent, string previewType, bool addDraftWatermark);
        Task<XsltListResponse> GetXsltListAsync();
        Task<PrefixCodeResponse> AddPrefixListAsync(string prefixType, string prefixKey);
        Task<EntResponse> UploadXsltAsync(string name, byte[] content, bool isDefault);
        Task<EntResponse> SetDefaultXsltAsync(string xsltName);
        Task<PrefixCodeResponse> SetPrefixStatusListAsync(string prefixType, string prefixKey, bool active);
    }
}
