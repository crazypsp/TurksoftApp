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
    public class EArchiveVoucherBusiness : BaseBusiness, IEArchiveVoucherBusiness
    {
        private const string ServiceUrl = "https://servis.kolayentegrasyon.net/EArchiveVoucherService/EArchiveVoucherWS";

        public EArchiveVoucherBusiness(string username, string password)
            : base(username, password) { }

        public List<EntResponse> SendSMM(List<InputDocument> vouchers) => new();
        public List<EntResponse> SendMM(List<InputDocument> vouchers) => new();
        public List<EntResponse> UpdateSMM(List<InputDocument> vouchers) => new();
        public List<EntResponse> UpdateMM(List<InputDocument> vouchers) => new();
        public EntResponse CancelSMM(string uuid, string reason, DateTime cancelDate) => new();
        public EntResponse CancelMM(string uuid, string reason, DateTime cancelDate) => new();
        public DocumentQueryResponse GetLastSMMIdAndDate(string sourceId, List<string> prefixes) => new();
        public DocumentQueryResponse GetLastMMIdAndDate(string sourceId, List<string> prefixes) => new();
        public DocumentQueryResponse QueryVouchers(string paramType, string parameter, string voucherType, string withXML) => new();
        public EntResponse SetSmmEmailSent(List<string> uuids) => new();
        public EntResponse SetMmEmailSent(List<string> uuids) => new();
        public CreditInfo GetCustomerCreditCount(string vknTckn) => new();
        public EntResponse SetSmmDocumentFlag(FlagSetter flagSetter) => new();
        public EntResponse SetMmDocumentFlag(FlagSetter flagSetter) => new();
        public EntResponse ControlXmlSmm(string xml) => new();
        public EntResponse ControlXmlMm(string xml) => new();
        public DocumentQueryResponse QueryVouchersWithLocalId(string localId, string voucherType) => new();
        public DocumentQueryResponse QueryVouchersWithDocumentDate(string start, string end, string voucherType, string withXML, string minRecordId) => new();
        public DocumentQueryResponse QueryVouchersWithReceivedDate(string start, string end, string voucherType, string withXML, string minRecordId) => new();
        public DocumentQueryResponse QueryVouchersWithGUIDList(List<string> guidList, string voucherType) => new();
        public DocumentQueryResponse IsEFaturaUser(string vknTckn) => new();
        public DocumentQueryResponse GetCreditActionsForCustomer(string vknTckn) => new();
    }
}
