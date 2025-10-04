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
    public class EArchiveVoucherService: IEArchiveVoucherService
    {
        private readonly IEArchiveVoucherBusiness _business;

        public EArchiveVoucherService(IEArchiveVoucherBusiness business)
        {
            _business = business;
        }

        public Task<List<EntResponse>> SendSMMAsync(List<InputDocument> vouchers)
            => Task.Run(() => _business.SendSMM(vouchers));

        public Task<List<EntResponse>> SendMMAsync(List<InputDocument> vouchers)
            => Task.Run(() => _business.SendMM(vouchers));

        public Task<List<EntResponse>> UpdateSMMAsync(List<InputDocument> vouchers)
            => Task.Run(() => _business.UpdateSMM(vouchers));

        public Task<List<EntResponse>> UpdateMMAsync(List<InputDocument> vouchers)
            => Task.Run(() => _business.UpdateMM(vouchers));

        public Task<EntResponse> CancelSMMAsync(string voucherUuid, string cancelReason, DateTime cancelDate)
            => Task.Run(() => _business.CancelSMM(voucherUuid, cancelReason, cancelDate));

        public Task<EntResponse> CancelMMAsync(string voucherUuid, string cancelReason, DateTime cancelDate)
            => Task.Run(() => _business.CancelMM(voucherUuid, cancelReason, cancelDate));

        public Task<DocumentQueryResponse> GetLastSMMIdAndDateAsync(string sourceId, List<string> documentIdPrefixList)
            => Task.Run(() => _business.GetLastSMMIdAndDate(sourceId, documentIdPrefixList));

        public Task<DocumentQueryResponse> GetLastMMIdAndDateAsync(string sourceId, List<string> documentIdPrefixList)
            => Task.Run(() => _business.GetLastMMIdAndDate(sourceId, documentIdPrefixList));

        public Task<DocumentQueryResponse> QueryVouchersAsync(string paramType, string parameter, string voucherType, string withXML)
            => Task.Run(() => _business.QueryVouchers(paramType, parameter, voucherType, withXML));

        public Task<EntResponse> SetSmmEmailSentAsync(List<string> voucherUuidList)
            => Task.Run(() => _business.SetSmmEmailSent(voucherUuidList));

        public Task<EntResponse> SetMmEmailSentAsync(List<string> voucherUuidList)
            => Task.Run(() => _business.SetMmEmailSent(voucherUuidList));

        public Task<CreditInfo> GetCustomerCreditCountAsync(string vknTckn)
            => Task.Run(() => _business.GetCustomerCreditCount(vknTckn));

        public Task<EntResponse> SetSmmDocumentFlagAsync(FlagSetter flagSetter)
            => Task.Run(() => _business.SetSmmDocumentFlag(flagSetter));

        public Task<EntResponse> SetMmDocumentFlagAsync(FlagSetter flagSetter)
            => Task.Run(() => _business.SetMmDocumentFlag(flagSetter));

        public Task<EntResponse> ControlXmlSmmAsync(string voucherXml)
            => Task.Run(() => _business.ControlXmlSmm(voucherXml));

        public Task<EntResponse> ControlXmlMmAsync(string voucherXml)
            => Task.Run(() => _business.ControlXmlMm(voucherXml));

        public Task<DocumentQueryResponse> QueryVouchersWithLocalIdAsync(string localId, string voucherType)
            => Task.Run(() => _business.QueryVouchersWithLocalId(localId, voucherType));

        public Task<DocumentQueryResponse> QueryVouchersWithDocumentDateAsync(string startDate, string endDate, string voucherType, string withXML, string minRecordId)
            => Task.Run(() => _business.QueryVouchersWithDocumentDate(startDate, endDate, voucherType, withXML, minRecordId));

        public Task<DocumentQueryResponse> QueryVouchersWithReceivedDateAsync(string startDate, string endDate, string voucherType, string withXML, string minRecordId)
            => Task.Run(() => _business.QueryVouchersWithReceivedDate(startDate, endDate, voucherType, withXML, minRecordId));

        public Task<DocumentQueryResponse> QueryVouchersWithGUIDListAsync(List<string> guidList, string voucherType)
            => Task.Run(() => _business.QueryVouchersWithGUIDList(guidList, voucherType));

        public Task<DocumentQueryResponse> IsEFaturaUserAsync(string vknTckn)
            => Task.Run(() => _business.IsEFaturaUser(vknTckn));

        public Task<DocumentQueryResponse> GetCreditActionsForCustomerAsync(string vknTckn)
            => Task.Run(() => _business.GetCreditActionsForCustomer(vknTckn));
    }
}
