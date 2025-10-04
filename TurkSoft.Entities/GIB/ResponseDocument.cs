using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIB
{
    public class ResponseDocument
    {
        public string DocumentUUID { get; set; }
        public string DocumentId { get; set; }
        public string EnvelopeUUID { get; set; }
        public string DocumentProfile { get; set; }
        public DateTime? SystemCreationTime { get; set; }
        public DateTime? DocumentIssueDate { get; set; }
        public string SourceId { get; set; }
        public string SourceUrn { get; set; }
        public string SourceTitle { get; set; }
        public string DestinationId { get; set; }
        public string DestinationUrn { get; set; }
        public string StateCode { get; set; }
        public string StateExplanation { get; set; }
        public string ContentType { get; set; }
        public byte[] DocumentContent { get; set; }
        public string CurrencyCode { get; set; }
        public string Cause { get; set; }
        public decimal? InvoiceTotal { get; set; }
        public string DocumentTypeCode { get; set; }
        public List<string> Notes { get; set; } = new();
        public decimal? TaxInclusiveAmount { get; set; }
        public decimal? TaxExclusiveAmount { get; set; }
        public decimal? AllowanceTotalAmount { get; set; }
        public decimal? TaxAmount0015 { get; set; }
        public decimal? LineExtensionAmount { get; set; }
        public string SupplierPersonName { get; set; }
        public string SupplierPersonMiddleName { get; set; }
        public string SupplierPersonFamilyName { get; set; }
        public string CustomerPersonName { get; set; }
        public string CustomerPersonMiddleName { get; set; }
        public string CustomerPersonFamilyName { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsArchived { get; set; }
        public bool? IsAccounted { get; set; }
        public bool? IsTransferred { get; set; }
        public bool? IsPrinted { get; set; }
        public string LocalId { get; set; }
        public string SendingType { get; set; }
    }
}
