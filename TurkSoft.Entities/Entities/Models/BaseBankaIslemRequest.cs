using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.Entities.Models
{
    public class BaseBankaIslemRequest
    {
        public string IslemKodu { get; set; }
        public DateTime Tarih { get; set; }
        public string FisNo { get; set; }
        public decimal Tutar { get; set; }
        public string BankaHesapKodu { get; set; }
        public int OlusturanKullanici { get; set; }
        public string ParaBirimi { get; set; }
        public decimal Kur { get; set; }
        public int DataReference { get; set; }
        public string CariKodu { get; set; }
        public string GLKodu { get; set; }
        public string GUID { get; set; }

        // BaseBankaIslemRequest içine EKLE
        public string Notes1 { get; set; }          // BANK_VOUCHER/NOTES1
        public string Description { get; set; }     // TRANSACTION/DESCRIPTION
        public string GL_CODE1 { get; set; }        // TRANSACTION/GL_CODE1
        public string GL_CODE2 { get; set; }        // TRANSACTION/GL_CODE2
        public string BN_COST_GL_CODE { get; set; } // TRANSACTION/BN_COST_GL_CODE
        public string BN_BSMV_GL_CODE { get; set; } // TRANSACTION/BN_BSMV_GL_CODE

        // Opsiyonel modify alanları istersen:
        public int? ModifiedBy { get; set; }
        public DateTime? DateModified { get; set; }
        public int? HourModified { get; set; }
        public int? MinModified { get; set; }
        public int? SecModified { get; set; }

    }
    public class GelenHavaleRequest : BaseBankaIslemRequest
    {
        public int Saat { get; set; }
        public int Dakika { get; set; }
        public int Saniye { get; set; }
        public int CURRSEL_TOTALS { get; set; }
        public decimal RC_TOTAL_DEBIT { get; set; }
        public int TYPE { get; set; } = 3; // Gelen havale için sabit
        public int TRCODE { get; set; } = 3; // Gelen havale için sabit
        public int MODULENR { get; set; } = 7;
        public string ARP_CODE { get; set; }
        public int CURR_TRANS { get; set; }
        public decimal TC_XRATE { get; set; }
        public decimal TC_AMOUNT { get; set; }
        public decimal RC_XRATE { get; set; }
        public decimal RC_AMOUNT { get; set; }
        public int BANK_PROC_TYPE { get; set; } = 2;
        public DateTime DUE_DATE { get; set; }
        public int AFFECT_RISK { get; set; }
        public int BN_CRDTYPE { get; set; }
        public int DIVISION { get; set; }
        public int DEPARMENT { get; set; }

        // Payment List için
        public List<PaymentItemRequest> PaymentList { get; set; } = new List<PaymentItemRequest>();
    }

    public class GidenHavaleRequest : BaseBankaIslemRequest
    {
        public int Saat { get; set; }
        public int Dakika { get; set; }
        public int Saniye { get; set; }
        public int CURRSEL_TOTALS { get; set; }
        public decimal RC_TOTAL_CREDIT { get; set; }
        public int SIGN { get; set; } = 1;
        public int TYPE { get; set; } = 4; // Giden havale için sabit
        public int TRCODE { get; set; } = 4; // Giden havale için sabit
        public int MODULENR { get; set; } = 7;
        public string ARP_CODE { get; set; }
        public int CURR_TRANS { get; set; }
        public decimal TC_XRATE { get; set; }
        public decimal TC_AMOUNT { get; set; }
        public decimal RC_XRATE { get; set; }
        public decimal RC_AMOUNT { get; set; }
        public int BANK_PROC_TYPE { get; set; } = 2;
        public DateTime DUE_DATE { get; set; }
        public int AFFECT_RISK { get; set; }
        public string BN_COST_GL_CODE { get; set; }
        public string BN_BSMV_GL_CODE { get; set; }
        public int BN_CRDTYPE { get; set; }
        public int DIVISION { get; set; }
        public int DEPARMENT { get; set; }

        // Payment List için
        public List<PaymentItemRequest> PaymentList { get; set; } = new List<PaymentItemRequest>();
    }

    public class VirmanRequest : BaseBankaIslemRequest
    {
        public int Saat { get; set; }
        public int Dakika { get; set; }
        public int Saniye { get; set; }
        public int CURRSEL_TOTALS { get; set; }
        public int CURRSEL_DETAILS { get; set; }
        public decimal RC_TOTAL_DEBIT { get; set; }
        public decimal RC_TOTAL_CREDIT { get; set; }
        public int TYPE { get; set; } = 2; // Virman için sabit
        public int TRCODE { get; set; } = 2; // Virman için sabit
        public string HedefHesapKodu { get; set; }
        public string KrediKodu { get; set; }
        public int KrediReferans { get; set; }
        public int SIGN { get; set; }
        public int MODULENR { get; set; } = 7;
        public int CURR_TRANS { get; set; }
        public decimal TC_XRATE { get; set; }
        public decimal TC_AMOUNT { get; set; }
        public decimal RC_XRATE { get; set; }
        public decimal RC_AMOUNT { get; set; }
        public int CURRSEL_TRANS { get; set; }
        public int BANK_PROC_TYPE { get; set; }
        public int AFFECT_RISK { get; set; }
        public string BN_CRDTYPE { get; set; }
        public int DIVISION { get; set; }
        public int DEPARMENT { get; set; }
        public string BN_FIN_COST_GL_CODE { get; set; }
    }

    public class KrediTaksitRequest : BaseBankaIslemRequest
    {
        public int Saat { get; set; }
        public int Dakika { get; set; }
        public int Saniye { get; set; }
        public int CURRSEL_TOTALS { get; set; }
        public int CURRSEL_DETAILS { get; set; }
        public decimal RC_TOTAL_DEBIT { get; set; }
        public decimal RC_TOTAL_CREDIT { get; set; }
        public int TYPE { get; set; } = 1; // Kredi taksit için sabit
        public int TRCODE { get; set; } = 1; // Kredi taksit için sabit
        public string KrediHesapKodu { get; set; }
        public string KrediKodu { get; set; }
        public int TaksitNo { get; set; }
        public decimal AnaParaTutar { get; set; }
        public decimal FaizTutar { get; set; }
        public decimal BsmvTutar { get; set; }
        public decimal ToplamTutar { get; set; }
        public DateTime VadeTarihi { get; set; }
        public int KrediReferans { get; set; }
        public int SIGN { get; set; }
        public int MODULENR { get; set; } = 7;
        public int CURR_TRANS { get; set; }
        public decimal TC_XRATE { get; set; }
        public decimal TC_AMOUNT { get; set; }
        public decimal RC_XRATE { get; set; }
        public decimal RC_AMOUNT { get; set; }
        public int CURRSEL_TRANS { get; set; }
        public int BANK_PROC_TYPE { get; set; }
        public int AFFECT_RISK { get; set; }
        public int BNK_CRE_SOURCE { get; set; }
        public int BNK_CRE_LINE_TYPE { get; set; }
        public string BN_CRDTYPE { get; set; }
        public int DIVISION { get; set; }
        public int DEPARMENT { get; set; }
        public string BN_FIN_COST_GL_CODE { get; set; }
        public string BN_INT_GL_CODE { get; set; }

        // Payment List için
        public List<PaymentItemRequest> PaymentList { get; set; } = new List<PaymentItemRequest>();

        // BNCREPAYMENTLIST için
        public List<BnkCrePaymentItemRequest> BnkCrePaymentList { get; set; } = new List<BnkCrePaymentItemRequest>();
    }

    public class PaymentItemRequest
    {
        public DateTime DATE { get; set; }
        public int MODULENR { get; set; }
        public int SIGN { get; set; }
        public int TRCODE { get; set; }
        public decimal TOTAL { get; set; }
        public DateTime PROCDATE { get; set; }
        public string TRCURR { get; set; }
        public decimal TRRATE { get; set; }
        public decimal REPORTRATE { get; set; }
        public int DATA_REFERENCE { get; set; }
        public DateTime DISCOUNT_DUEDATE { get; set; }
        public int DISCTRDELLIST { get; set; }
    }

    public class BnkCrePaymentItemRequest
    {
        public int PER_NR { get; set; }
        public int TRANS_TYPE { get; set; }
        public int PARENT_REF { get; set; }
        public DateTime DUE_DATE { get; set; }
        public DateTime OPR_DATE { get; set; }
        public int LINE_NR { get; set; }
        public decimal TOTAL { get; set; }
        public decimal INT_TOTAL { get; set; }
        public int BANK_FICHE_REF { get; set; }
        public decimal TR_RATE_CR { get; set; }
        public decimal TR_RATE_ACC { get; set; }
        public int CREATED_BY { get; set; }
        public DateTime DATE_CREATED { get; set; }
        public int HOUR_CREATED { get; set; }
        public int MIN_CREATED { get; set; }
        public int SEC_CREATED { get; set; }
        public string LN_ACC_CODE { get; set; }
    }
}
