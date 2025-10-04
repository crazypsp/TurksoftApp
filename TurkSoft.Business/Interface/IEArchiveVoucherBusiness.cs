using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.GIB;

namespace TurkSoft.Business.Interface
{
    public interface IEArchiveVoucherBusiness
    {
        List<EntResponse> SendSMM(List<InputDocument> vouchers);
        List<EntResponse> SendMM(List<InputDocument> vouchers);
        List<EntResponse> UpdateSMM(List<InputDocument> vouchers);
        List<EntResponse> UpdateMM(List<InputDocument> vouchers);
        EntResponse CancelSMM(string voucherUuid, string reason, DateTime cancelDate);
        EntResponse CancelMM(string voucherUuid, string reason, DateTime cancelDate);
        DocumentQueryResponse GetLastSMMIdAndDate(string sourceId, List<string> prefixes);
        DocumentQueryResponse GetLastMMIdAndDate(string sourceId, List<string> prefixes);
        DocumentQueryResponse QueryVouchers(string paramType, string parameter, string voucherType, string withXML);
        EntResponse SetSmmEmailSent(List<string> uuids);
        EntResponse SetMmEmailSent(List<string> uuids);
        CreditInfo GetCustomerCreditCount(string vknTckn);
        EntResponse SetSmmDocumentFlag(FlagSetter flagSetter);
        EntResponse SetMmDocumentFlag(FlagSetter flagSetter);
        EntResponse ControlXmlSmm(string xml);
        EntResponse ControlXmlMm(string xml);
        DocumentQueryResponse QueryVouchersWithLocalId(string localId, string voucherType);
        DocumentQueryResponse QueryVouchersWithDocumentDate(string start, string end, string voucherType, string withXML, string minRecordId);
        DocumentQueryResponse QueryVouchersWithReceivedDate(string start, string end, string voucherType, string withXML, string minRecordId);
        DocumentQueryResponse QueryVouchersWithGUIDList(List<string> guidList, string voucherType);
        DocumentQueryResponse IsEFaturaUser(string vknTckn);
        DocumentQueryResponse GetCreditActionsForCustomer(string vknTckn);
    }
}
