using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.GIB;

namespace TurkSoft.Business.Interface
{
    public interface IEArchiveInvoiceBusiness
    {
        List<EntResponse> SendInvoice(List<InputDocument> inputDocuments);
        List<EntResponse> UpdateInvoice(List<InputDocument> inputDocuments);
        EntResponse CancelInvoice(string invoiceUuid, string cancelReason, DateTime cancelDate);
        DocumentQueryResponse QueryInvoice(string paramType, string parameter, string withXML);
        EntResponse SetEmailSent(List<string> invoiceUuids);
        CreditInfo GetCustomerCreditCount(string vknTckn);
        CreditInfo GetCustomerCreditSpace(string vknTckn);
        EntResponse ControlInvoiceXML(string invoiceXML);
        DocumentQueryResponse QueryInvoiceWithLocalId(string localId);
        DocumentQueryResponse QueryInvoiceWithDocumentDate(string startDate, string endDate, string withXML, string minRecordId);
        DocumentQueryResponse QueryInvoiceWithReceivedDate(string startDate, string endDate, string withXML, string minRecordId);
        DocumentQueryResponse QueryInvoicesWithGUIDList(List<string> guidList);
        DocumentQueryResponse IsEFaturaUser(string vknTckn);
        DocumentQueryResponse GetCreditActionsForCustomer(string vknTckn);
        LogResponse GetEAInvoiceStatusWithLogs(List<string> documentUuids);
        DocumentQueryResponse QueryArchivedInvoice(string paramType, string parameter, string withXML, int fiscalYear);
        DocumentQueryResponse QueryArchivedInvoiceWithDocumentDate(string startDate, string endDate, string withXML, string minRecordId);
        EntResponse GetDraftDocumentPreview(string xmlContent, string previewType, bool addDraftWatermark);
        XsltListResponse GetXsltList();
        PrefixCodeResponse AddPrefixList(string prefixType, string prefixKey);
        EntResponse UploadXslt(string name, byte[] content, bool isDefault);
        EntResponse SetDefaultXslt(string xsltName);
        PrefixCodeResponse SetPrefixStatusList(string prefixType, string prefixKey, bool active);
    }
}
