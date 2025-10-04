using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.GIB;

namespace TurkSoft.Service.Interface
{
    public interface IEArchiveVoucherService
    {
        Task<List<EntResponse>> SendSMMAsync(List<InputDocument> vouchers);
        Task<List<EntResponse>> SendMMAsync(List<InputDocument> vouchers);
        Task<List<EntResponse>> UpdateSMMAsync(List<InputDocument> vouchers);
        Task<List<EntResponse>> UpdateMMAsync(List<InputDocument> vouchers);
        Task<EntResponse> CancelSMMAsync(string voucherUuid, string cancelReason, DateTime cancelDate);
        Task<EntResponse> CancelMMAsync(string voucherUuid, string cancelReason, DateTime cancelDate);
        Task<DocumentQueryResponse> GetLastSMMIdAndDateAsync(string sourceId, List<string> documentIdPrefixList);
        Task<DocumentQueryResponse> GetLastMMIdAndDateAsync(string sourceId, List<string> documentIdPrefixList);
        Task<DocumentQueryResponse> QueryVouchersAsync(string paramType, string parameter, string voucherType, string withXML);
        Task<EntResponse> SetSmmEmailSentAsync(List<string> voucherUuidList);
        Task<EntResponse> SetMmEmailSentAsync(List<string> voucherUuidList);
        Task<CreditInfo> GetCustomerCreditCountAsync(string vknTckn);
        Task<EntResponse> SetSmmDocumentFlagAsync(FlagSetter flagSetter);
        Task<EntResponse> SetMmDocumentFlagAsync(FlagSetter flagSetter);
        Task<EntResponse> ControlXmlSmmAsync(string voucherXml);
        Task<EntResponse> ControlXmlMmAsync(string voucherXml);
        Task<DocumentQueryResponse> QueryVouchersWithLocalIdAsync(string localId, string voucherType);
        Task<DocumentQueryResponse> QueryVouchersWithDocumentDateAsync(string startDate, string endDate, string voucherType, string withXML, string minRecordId);
        Task<DocumentQueryResponse> QueryVouchersWithReceivedDateAsync(string startDate, string endDate, string voucherType, string withXML, string minRecordId);
        Task<DocumentQueryResponse> QueryVouchersWithGUIDListAsync(List<string> guidList, string voucherType);
        Task<DocumentQueryResponse> IsEFaturaUserAsync(string vknTckn);
        Task<DocumentQueryResponse> GetCreditActionsForCustomerAsync(string vknTckn);
    }
}
