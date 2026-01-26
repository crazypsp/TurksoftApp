using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities.Models;
using TurkSoft.Service.Inferfaces;
using UnityObjects; // Logo Tiger UnityObjects namespace

namespace TurkSoft.Service.Implementations
{
    public class LogoTigerIntegrationService : ILogoTigerIntegrationService
    {
        private readonly ILogger<LogoTigerIntegrationService> _logger;
        public static UnityApplication UnityApp = new UnityApplication();

        public LogoTigerIntegrationService(ILogger<LogoTigerIntegrationService> logger)
        {
            _logger = logger;
        }

        public async Task<ServiceResult<BankaFisSonuc>> GelenHavaleEkleAsync(GelenHavaleRequest request)
        {
            try
            {
                _logger.LogInformation("Gelen havale işlemi başlatılıyor. İşlemKodu: {IslemKodu}", request.IslemKodu);

                // 1. Validasyon
                if (!ValidateGelenHavaleRequest(request, out string validationError))
                {
                    return ServiceResult<BankaFisSonuc>.ErrorResult($"Validasyon hatası: {validationError}");
                }

                // 2. UnityObjects Data Object oluştur
                var bankvo = UnityApp.NewDataObject(DataObjectType.doBankVoucher);
                bankvo.New();

                // 3. Tarih formatı (kullanıcıdan gelen tarih)
                string tarih = request.Tarih.ToString("dd.MM.yyyy");
                string dueDate = request.DUE_DATE.ToString("dd.MM.yyyy");

                // 4. Ana fiş bilgileri (TÜMÜ kullanıcıdan)
                bankvo.DataFields.FieldByName("DATE").Value = tarih;
                bankvo.DataFields.FieldByName("NUMBER").Value = request.FisNo;
                bankvo.DataFields.FieldByName("TYPE").Value = request.TYPE;
                bankvo.DataFields.FieldByName("TOTAL_DEBIT").Value = request.Tutar;
                bankvo.DataFields.FieldByName("CREATED_BY").Value = request.OlusturanKullanici;
                bankvo.DataFields.FieldByName("DATE_CREATED").Value = tarih;
                bankvo.DataFields.FieldByName("HOUR_CREATED").Value = request.Saat;
                bankvo.DataFields.FieldByName("MIN_CREATED").Value = request.Dakika;
                bankvo.DataFields.FieldByName("SEC_CREATED").Value = request.Saniye;
                bankvo.DataFields.FieldByName("CURRSEL_TOTALS").Value = request.CURRSEL_TOTALS;
                bankvo.DataFields.FieldByName("DATA_REFERENCE").Value = request.DataReference;
                bankvo.DataFields.FieldByName("RC_TOTAL_DEBIT").Value = request.RC_TOTAL_DEBIT;
                bankvo.DataFields.FieldByName("DIVISION").Value = request.DIVISION;
                bankvo.DataFields.FieldByName("DEPARMENT").Value = request.DEPARMENT;

                // 5. TRANSACTIONS satırı
                var transactions_lines = bankvo.DataFields.FieldByName("TRANSACTIONS").Lines;
                transactions_lines.AppendLine();

                // Transaction detayları (TÜMÜ kullanıcıdan)
                transactions_lines[transactions_lines.Count - 1].FieldByName("TYPE").Value = request.TYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRANNO").Value = request.FisNo;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANKACC_CODE").Value = request.BankaHesapKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("ARP_CODE").Value = request.ARP_CODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("SOURCEFREF").Value = request.DataReference;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DATE").Value = tarih;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRCODE").Value = request.TRCODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("MODULENR").Value = request.MODULENR;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURR_TRANS").Value = request.CURR_TRANS;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DEBIT").Value = request.Tutar;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AMOUNT").Value = request.Tutar;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_XRATE").Value = request.TC_XRATE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_AMOUNT").Value = request.TC_AMOUNT;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_XRATE").Value = request.RC_XRATE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_AMOUNT").Value = request.RC_AMOUNT;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANK_PROC_TYPE").Value = request.BANK_PROC_TYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DUE_DATE").Value = dueDate;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DATA_REFERENCE").Value = request.DataReference;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AFFECT_RISK").Value = request.AFFECT_RISK;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BN_CRDTYPE").Value = request.BN_CRDTYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DIVISION").Value = request.DIVISION;

                // GUID (kullanıcıdan alınan veya oluşturulan)
                transactions_lines[transactions_lines.Count - 1].FieldByName("GUID").Value =
                    string.IsNullOrEmpty(request.GUID) ? Guid.NewGuid().ToString().ToUpper() : request.GUID;

                bankvo.DataFields.FieldByName("GUID").Value =
                    string.IsNullOrEmpty(request.GUID) ? Guid.NewGuid().ToString().ToUpper() : request.GUID;

                // 6. PAYMENT_LIST ekleme (kullanıcıdan gelen liste)
                var payment_list = transactions_lines[transactions_lines.Count - 1].FieldByName("PAYMENT_LIST").Lines;
                foreach (var payment in request.PaymentList)
                {
                    payment_list.AppendLine();
                    int lastIndex = payment_list.Count - 1;

                    payment_list[lastIndex].FieldByName("DATE").Value = payment.DATE.ToString("dd.MM.yyyy");
                    payment_list[lastIndex].FieldByName("MODULENR").Value = payment.MODULENR;
                    payment_list[lastIndex].FieldByName("SIGN").Value = payment.SIGN;
                    payment_list[lastIndex].FieldByName("TRCODE").Value = payment.TRCODE;
                    payment_list[lastIndex].FieldByName("TOTAL").Value = payment.TOTAL;
                    payment_list[lastIndex].FieldByName("PROCDATE").Value = payment.PROCDATE.ToString("dd.MM.yyyy");
                    payment_list[lastIndex].FieldByName("TRCURR").Value = payment.TRCURR;
                    payment_list[lastIndex].FieldByName("TRRATE").Value = payment.TRRATE;
                    payment_list[lastIndex].FieldByName("REPORTRATE").Value = payment.REPORTRATE;
                    payment_list[lastIndex].FieldByName("DATA_REFERENCE").Value = payment.DATA_REFERENCE;
                    payment_list[lastIndex].FieldByName("DISCOUNT_DUEDATE").Value = payment.DISCOUNT_DUEDATE.ToString("dd.MM.yyyy");
                    payment_list[lastIndex].FieldByName("DISCTRDELLIST").Value = payment.DISCTRDELLIST;
                }

                // 7. Post işlemi
                bool postResult = bankvo.Post();

                if (postResult)
                {
                    int fisRef = Convert.ToInt32(bankvo.DataFields.FieldByName("LOGICALREF").Value);

                    _logger.LogInformation("Gelen havale fişi başarıyla oluşturuldu. FisRef: {FisRef}", fisRef);

                    var sonuc = new BankaFisSonuc
                    {
                        Basarili = true,
                        FisReferans = fisRef,
                        Mesaj = "Gelen havale fişi başarıyla oluşturuldu.",
                        IslemTarihi = DateTime.Now,
                        IslemKodu = request.IslemKodu,
                        GUID = bankvo.DataFields.FieldByName("GUID").Value.ToString()
                    };

                    return ServiceResult<BankaFisSonuc>.SuccessResult(sonuc);
                }
                else
                {
                    return HandleLogoErrors<BankaFisSonuc>(bankvo, request.IslemKodu);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gelen havale işleminde beklenmeyen hata: {IslemKodu}", request.IslemKodu);
                return ServiceResult<BankaFisSonuc>.ErrorResult($"Sistem hatası: {ex.Message}");
            }
        }

        public async Task<ServiceResult<BankaFisSonuc>> GidenHavaleEkleAsync(GidenHavaleRequest request)
        {
            try
            {
                _logger.LogInformation("Giden havale işlemi başlatılıyor. İşlemKodu: {IslemKodu}", request.IslemKodu);

                // Validasyon
                if (!ValidateGidenHavaleRequest(request, out string validationError))
                {
                    return ServiceResult<BankaFisSonuc>.ErrorResult($"Validasyon hatası: {validationError}");
                }

                var bankvo = UnityApp.NewDataObject(DataObjectType.doBankVoucher);
                bankvo.New();

                string tarih = request.Tarih.ToString("dd.MM.yyyy");
                string dueDate = request.DUE_DATE.ToString("dd.MM.yyyy");

                // Ana fiş bilgileri (TÜMÜ kullanıcıdan)
                bankvo.DataFields.FieldByName("DATE").Value = tarih;
                bankvo.DataFields.FieldByName("NUMBER").Value = request.FisNo;
                bankvo.DataFields.FieldByName("TYPE").Value = request.TYPE;
                bankvo.DataFields.FieldByName("SIGN").Value = request.SIGN;
                bankvo.DataFields.FieldByName("TOTAL_CREDIT").Value = request.Tutar;
                bankvo.DataFields.FieldByName("CREATED_BY").Value = request.OlusturanKullanici;
                bankvo.DataFields.FieldByName("DATE_CREATED").Value = tarih;
                bankvo.DataFields.FieldByName("HOUR_CREATED").Value = request.Saat;
                bankvo.DataFields.FieldByName("MIN_CREATED").Value = request.Dakika;
                bankvo.DataFields.FieldByName("SEC_CREATED").Value = request.Saniye;
                bankvo.DataFields.FieldByName("CURRSEL_TOTALS").Value = request.CURRSEL_TOTALS;
                bankvo.DataFields.FieldByName("DATA_REFERENCE").Value = request.DataReference;
                bankvo.DataFields.FieldByName("RC_TOTAL_CREDIT").Value = request.RC_TOTAL_CREDIT;
                bankvo.DataFields.FieldByName("DIVISION").Value = request.DIVISION;
                bankvo.DataFields.FieldByName("DEPARMENT").Value = request.DEPARMENT;

                // TRANSACTIONS
                var transactions_lines = bankvo.DataFields.FieldByName("TRANSACTIONS").Lines;
                transactions_lines.AppendLine();

                // Transaction detayları (TÜMÜ kullanıcıdan)
                transactions_lines[transactions_lines.Count - 1].FieldByName("TYPE").Value = 1; // Giden havale için TYPE=1
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRANNO").Value = request.FisNo;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANKACC_CODE").Value = request.BankaHesapKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("ARP_CODE").Value = request.ARP_CODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("GL_CODE2").Value = request.GLKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("SOURCEFREF").Value = request.DataReference;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DATE").Value = tarih;
                transactions_lines[transactions_lines.Count - 1].FieldByName("SIGN").Value = request.SIGN;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRCODE").Value = request.TRCODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("MODULENR").Value = request.MODULENR;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURR_TRANS").Value = request.CURR_TRANS;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CREDIT").Value = request.Tutar;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AMOUNT").Value = request.Tutar;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_XRATE").Value = request.TC_XRATE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_AMOUNT").Value = request.TC_AMOUNT;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_XRATE").Value = request.RC_XRATE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_AMOUNT").Value = request.RC_AMOUNT;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANK_PROC_TYPE").Value = request.BANK_PROC_TYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DUE_DATE").Value = dueDate;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DATA_REFERENCE").Value = request.DataReference;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AFFECT_RISK").Value = request.AFFECT_RISK;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BN_COST_GL_CODE").Value = request.BN_COST_GL_CODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BN_BSMV_GL_CODE").Value = request.BN_BSMV_GL_CODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BN_CRDTYPE").Value = request.BN_CRDTYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DIVISION").Value = request.DIVISION;

                // GUID
                transactions_lines[transactions_lines.Count - 1].FieldByName("GUID").Value =
                    string.IsNullOrEmpty(request.GUID) ? Guid.NewGuid().ToString().ToUpper() : request.GUID;

                bankvo.DataFields.FieldByName("GUID").Value =
                    string.IsNullOrEmpty(request.GUID) ? Guid.NewGuid().ToString().ToUpper() : request.GUID;

                // PAYMENT_LIST
                var payment_list = transactions_lines[transactions_lines.Count - 1].FieldByName("PAYMENT_LIST").Lines;
                foreach (var payment in request.PaymentList)
                {
                    payment_list.AppendLine();
                    int lastIndex = payment_list.Count - 1;

                    payment_list[lastIndex].FieldByName("DATE").Value = payment.DATE.ToString("dd.MM.yyyy");
                    payment_list[lastIndex].FieldByName("MODULENR").Value = payment.MODULENR;
                    payment_list[lastIndex].FieldByName("TRCODE").Value = payment.TRCODE;
                    payment_list[lastIndex].FieldByName("TOTAL").Value = payment.TOTAL;
                    payment_list[lastIndex].FieldByName("PROCDATE").Value = payment.PROCDATE.ToString("dd.MM.yyyy");
                    payment_list[lastIndex].FieldByName("TRCURR").Value = payment.TRCURR;
                    payment_list[lastIndex].FieldByName("TRRATE").Value = payment.TRRATE;
                    payment_list[lastIndex].FieldByName("REPORTRATE").Value = payment.REPORTRATE;
                    payment_list[lastIndex].FieldByName("DATA_REFERENCE").Value = payment.DATA_REFERENCE;
                    payment_list[lastIndex].FieldByName("DISCOUNT_DUEDATE").Value = payment.DISCOUNT_DUEDATE.ToString("dd.MM.yyyy");
                    payment_list[lastIndex].FieldByName("DISCTRDELLIST").Value = payment.DISCTRDELLIST;
                }

                bool postResult = bankvo.Post();

                if (postResult)
                {
                    int fisRef = Convert.ToInt32(bankvo.DataFields.FieldByName("LOGICALREF").Value);

                    _logger.LogInformation("Giden havale fişi başarıyla oluşturuldu. FisRef: {FisRef}", fisRef);

                    var sonuc = new BankaFisSonuc
                    {
                        Basarili = true,
                        FisReferans = fisRef,
                        Mesaj = "Giden havale fişi başarıyla oluşturuldu.",
                        IslemTarihi = DateTime.Now,
                        IslemKodu = request.IslemKodu,
                        GUID = bankvo.DataFields.FieldByName("GUID").Value.ToString()
                    };

                    return ServiceResult<BankaFisSonuc>.SuccessResult(sonuc);
                }
                else
                {
                    return HandleLogoErrors<BankaFisSonuc>(bankvo, request.IslemKodu);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giden havale işleminde beklenmeyen hata: {IslemKodu}", request.IslemKodu);
                return ServiceResult<BankaFisSonuc>.ErrorResult($"Sistem hatası: {ex.Message}");
            }
        }

        public async Task<ServiceResult<BankaFisSonuc>> VirmanEkleAsync(VirmanRequest request)
        {
            try
            {
                _logger.LogInformation("Virman işlemi başlatılıyor. İşlemKodu: {IslemKodu}", request.IslemKodu);

                // Validasyon
                if (!ValidateVirmanRequest(request, out string validationError))
                {
                    return ServiceResult<BankaFisSonuc>.ErrorResult($"Validasyon hatası: {validationError}");
                }

                var bankvo = UnityApp.NewDataObject(DataObjectType.doBankVoucher);
                bankvo.New();

                string tarih = request.Tarih.ToString("dd.MM.yyyy");

                // Ana fiş bilgileri (TÜMÜ kullanıcıdan)
                bankvo.DataFields.FieldByName("DATE").Value = tarih;
                bankvo.DataFields.FieldByName("NUMBER").Value = request.FisNo;
                bankvo.DataFields.FieldByName("TYPE").Value = request.TYPE;
                bankvo.DataFields.FieldByName("TOTAL_DEBIT").Value = request.Tutar;
                bankvo.DataFields.FieldByName("TOTAL_CREDIT").Value = request.Tutar;
                bankvo.DataFields.FieldByName("CREATED_BY").Value = request.OlusturanKullanici;
                bankvo.DataFields.FieldByName("DATE_CREATED").Value = tarih;
                bankvo.DataFields.FieldByName("HOUR_CREATED").Value = request.Saat;
                bankvo.DataFields.FieldByName("MIN_CREATED").Value = request.Dakika;
                bankvo.DataFields.FieldByName("SEC_CREATED").Value = request.Saniye;
                bankvo.DataFields.FieldByName("CURRSEL_TOTALS").Value = request.CURRSEL_TOTALS;
                bankvo.DataFields.FieldByName("CURRSEL_DETAILS").Value = request.CURRSEL_DETAILS;
                bankvo.DataFields.FieldByName("DATA_REFERENCE").Value = request.DataReference;
                bankvo.DataFields.FieldByName("RC_TOTAL_DEBIT").Value = request.RC_TOTAL_DEBIT;
                bankvo.DataFields.FieldByName("RC_TOTAL_CREDIT").Value = request.RC_TOTAL_CREDIT;
                bankvo.DataFields.FieldByName("BANK_CREDIT_CODE").Value = request.KrediKodu;
                bankvo.DataFields.FieldByName("BNCREREF").Value = request.KrediReferans;
                bankvo.DataFields.FieldByName("DIVISION").Value = request.DIVISION;
                bankvo.DataFields.FieldByName("DEPARMENT").Value = request.DEPARMENT;

                // İki TRANSACTION satırı
                var transactions_lines = bankvo.DataFields.FieldByName("TRANSACTIONS").Lines;

                // 1. Kaynak hesaptan çıkış
                transactions_lines.AppendLine();
                transactions_lines[transactions_lines.Count - 1].FieldByName("TYPE").Value = 6;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRANNO").Value = request.FisNo;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANKACC_CODE").Value = request.BankaHesapKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("SOURCEFREF").Value = request.DataReference;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DATE").Value = tarih;
                transactions_lines[transactions_lines.Count - 1].FieldByName("SIGN").Value = request.SIGN;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRCODE").Value = request.TRCODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("MODULENR").Value = request.MODULENR;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURR_TRANS").Value = request.CURR_TRANS;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CREDIT").Value = request.Tutar;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AMOUNT").Value = request.Tutar;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_XRATE").Value = request.TC_XRATE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_AMOUNT").Value = request.TC_AMOUNT;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_XRATE").Value = request.RC_XRATE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_AMOUNT").Value = request.RC_AMOUNT;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURRSEL_TRANS").Value = request.CURRSEL_TRANS;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANK_PROC_TYPE").Value = request.BANK_PROC_TYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DATA_REFERENCE").Value = request.DataReference;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AFFECT_RISK").Value = request.AFFECT_RISK;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BN_CRDTYPE").Value = request.BN_CRDTYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DIVISION").Value = request.DIVISION;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANK_CREDIT_CODE").Value = request.KrediKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("GUID").Value =
                    string.IsNullOrEmpty(request.GUID) ? Guid.NewGuid().ToString().ToUpper() : request.GUID;

                // 2. Hedef hesaba giriş
                transactions_lines.AppendLine();
                transactions_lines[transactions_lines.Count - 1].FieldByName("TYPE").Value = 1;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRANNO").Value = request.FisNo;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANKACC_CODE").Value = request.HedefHesapKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("GL_CODE2").Value = request.GLKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("SOURCEFREF").Value = request.DataReference;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DATE").Value = tarih;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRCODE").Value = request.TRCODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("MODULENR").Value = request.MODULENR;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURR_TRANS").Value = request.CURR_TRANS;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DEBIT").Value = request.Tutar;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AMOUNT").Value = request.Tutar;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_XRATE").Value = request.TC_XRATE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_AMOUNT").Value = request.TC_AMOUNT;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_XRATE").Value = request.RC_XRATE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_AMOUNT").Value = request.RC_AMOUNT;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURRSEL_TRANS").Value = request.CURRSEL_TRANS;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANK_PROC_TYPE").Value = request.BANK_PROC_TYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DATA_REFERENCE").Value = request.DataReference;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AFFECT_RISK").Value = request.AFFECT_RISK;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BN_CRDTYPE").Value = request.BN_CRDTYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DIVISION").Value = request.DIVISION;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BN_FIN_COST_GL_CODE").Value = request.BN_FIN_COST_GL_CODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANK_CREDIT_CODE").Value = request.KrediKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("GUID").Value =
                    string.IsNullOrEmpty(request.GUID) ? Guid.NewGuid().ToString().ToUpper() : request.GUID;

                bankvo.DataFields.FieldByName("GUID").Value =
                    string.IsNullOrEmpty(request.GUID) ? Guid.NewGuid().ToString().ToUpper() : request.GUID;

                bool postResult = bankvo.Post();

                if (postResult)
                {
                    int fisRef = Convert.ToInt32(bankvo.DataFields.FieldByName("LOGICALREF").Value);

                    _logger.LogInformation("Virman fişi başarıyla oluşturuldu. FisRef: {FisRef}", fisRef);

                    var sonuc = new BankaFisSonuc
                    {
                        Basarili = true,
                        FisReferans = fisRef,
                        Mesaj = "Virman fişi başarıyla oluşturuldu.",
                        IslemTarihi = DateTime.Now,
                        IslemKodu = request.IslemKodu,
                        GUID = bankvo.DataFields.FieldByName("GUID").Value.ToString()
                    };

                    return ServiceResult<BankaFisSonuc>.SuccessResult(sonuc);
                }
                else
                {
                    return HandleLogoErrors<BankaFisSonuc>(bankvo, request.IslemKodu);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Virman işleminde beklenmeyen hata: {IslemKodu}", request.IslemKodu);
                return ServiceResult<BankaFisSonuc>.ErrorResult($"Sistem hatası: {ex.Message}");
            }
        }

        public async Task<ServiceResult<KrediTaksitSonuc>> KrediTaksitOdemeEkleAsync(KrediTaksitRequest request)
        {
            try
            {
                _logger.LogInformation("Kredi taksit ödeme işlemi başlatılıyor. İşlemKodu: {IslemKodu}", request.IslemKodu);

                // Validasyon
                if (!ValidateKrediTaksitRequest(request, out string validationError))
                {
                    return ServiceResult<KrediTaksitSonuc>.ErrorResult($"Validasyon hatası: {validationError}");
                }

                var bankvo = UnityApp.NewDataObject(DataObjectType.doBankVoucher);
                bankvo.New();

                string tarih = request.Tarih.ToString("dd.MM.yyyy");
                string vadeTarihi = request.VadeTarihi.ToString("dd.MM.yyyy");
                decimal toplamTutar = request.ToplamTutar;

                // Ana fiş bilgileri (TÜMÜ kullanıcıdan)
                bankvo.DataFields.FieldByName("DATE").Value = tarih;
                bankvo.DataFields.FieldByName("NUMBER").Value = request.FisNo;
                bankvo.DataFields.FieldByName("TYPE").Value = request.TYPE;
                bankvo.DataFields.FieldByName("TOTAL_DEBIT").Value = toplamTutar;
                bankvo.DataFields.FieldByName("TOTAL_CREDIT").Value = toplamTutar;
                bankvo.DataFields.FieldByName("CREATED_BY").Value = request.OlusturanKullanici;
                bankvo.DataFields.FieldByName("DATE_CREATED").Value = tarih;
                bankvo.DataFields.FieldByName("HOUR_CREATED").Value = request.Saat;
                bankvo.DataFields.FieldByName("MIN_CREATED").Value = request.Dakika;
                bankvo.DataFields.FieldByName("SEC_CREATED").Value = request.Saniye;
                bankvo.DataFields.FieldByName("CURRSEL_TOTALS").Value = request.CURRSEL_TOTALS;
                bankvo.DataFields.FieldByName("CURRSEL_DETAILS").Value = request.CURRSEL_DETAILS;
                bankvo.DataFields.FieldByName("DATA_REFERENCE").Value = request.DataReference;
                bankvo.DataFields.FieldByName("RC_TOTAL_DEBIT").Value = request.RC_TOTAL_DEBIT;
                bankvo.DataFields.FieldByName("RC_TOTAL_CREDIT").Value = request.RC_TOTAL_CREDIT;
                bankvo.DataFields.FieldByName("BANK_CREDIT_CODE").Value = request.KrediKodu;
                bankvo.DataFields.FieldByName("BNCREREF").Value = request.KrediReferans;
                bankvo.DataFields.FieldByName("DIVISION").Value = request.DIVISION;
                bankvo.DataFields.FieldByName("DEPARMENT").Value = request.DEPARMENT;

                // 3 TRANSACTION satırı
                var transactions_lines = bankvo.DataFields.FieldByName("TRANSACTIONS").Lines;

                // 1. Ana para ödemesi
                transactions_lines.AppendLine();
                transactions_lines[transactions_lines.Count - 1].FieldByName("TYPE").Value = 1;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRANNO").Value = request.FisNo;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANKACC_CODE").Value = request.BankaHesapKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("GL_CODE2").Value = request.GLKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("SOURCEFREF").Value = request.DataReference;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DATE").Value = tarih;
                transactions_lines[transactions_lines.Count - 1].FieldByName("SIGN").Value = request.SIGN;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRCODE").Value = request.TRCODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("MODULENR").Value = request.MODULENR;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURR_TRANS").Value = request.CURR_TRANS;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CREDIT").Value = request.AnaParaTutar;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AMOUNT").Value = request.AnaParaTutar;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_XRATE").Value = request.TC_XRATE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_AMOUNT").Value = request.TC_AMOUNT;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_XRATE").Value = request.RC_XRATE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_AMOUNT").Value = request.RC_AMOUNT;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURRSEL_TRANS").Value = request.CURRSEL_TRANS;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANK_PROC_TYPE").Value = request.BANK_PROC_TYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DATA_REFERENCE").Value = request.DataReference;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AFFECT_RISK").Value = request.AFFECT_RISK;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BNK_CRE_SOURCE").Value = request.BNK_CRE_SOURCE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BN_CRDTYPE").Value = request.BN_CRDTYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DIVISION").Value = request.DIVISION;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BN_FIN_COST_GL_CODE").Value = request.BN_FIN_COST_GL_CODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANK_CREDIT_CODE").Value = request.KrediKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("GUID").Value =
                    string.IsNullOrEmpty(request.GUID) ? Guid.NewGuid().ToString().ToUpper() : request.GUID;

                // 2. Kredi hesabına toplam ödeme
                transactions_lines.AppendLine();
                transactions_lines[transactions_lines.Count - 1].FieldByName("TYPE").Value = 6;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRANNO").Value = request.FisNo;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANKACC_CODE").Value = request.KrediHesapKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("SOURCEFREF").Value = request.DataReference;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DATE").Value = tarih;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRCODE").Value = request.TRCODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("MODULENR").Value = request.MODULENR;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURR_TRANS").Value = request.CURR_TRANS;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DEBIT").Value = toplamTutar;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AMOUNT").Value = toplamTutar;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_XRATE").Value = request.TC_XRATE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_AMOUNT").Value = request.TC_AMOUNT;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_XRATE").Value = request.RC_XRATE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_AMOUNT").Value = request.RC_AMOUNT;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURRSEL_TRANS").Value = request.CURRSEL_TRANS;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANK_PROC_TYPE").Value = request.BANK_PROC_TYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DATA_REFERENCE").Value = request.DataReference;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AFFECT_RISK").Value = request.AFFECT_RISK;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BNK_CRE_SOURCE").Value = request.BNK_CRE_SOURCE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BN_CRDTYPE").Value = request.BN_CRDTYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DIVISION").Value = request.DIVISION;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANK_CREDIT_CODE").Value = request.KrediKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("GUID").Value =
                    string.IsNullOrEmpty(request.GUID) ? Guid.NewGuid().ToString().ToUpper() : request.GUID;

                // 3. Faiz ödemesi
                transactions_lines.AppendLine();
                transactions_lines[transactions_lines.Count - 1].FieldByName("TYPE").Value = 1;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRANNO").Value = request.FisNo;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANKACC_CODE").Value = request.BankaHesapKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("GL_CODE2").Value = request.GLKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("SOURCEFREF").Value = request.DataReference;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DATE").Value = tarih;
                transactions_lines[transactions_lines.Count - 1].FieldByName("SIGN").Value = request.SIGN;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TRCODE").Value = request.TRCODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("MODULENR").Value = request.MODULENR;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURR_TRANS").Value = request.CURR_TRANS;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CREDIT").Value = request.FaizTutar;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AMOUNT").Value = request.FaizTutar;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_XRATE").Value = request.TC_XRATE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("TC_AMOUNT").Value = request.TC_AMOUNT;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_XRATE").Value = request.RC_XRATE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("RC_AMOUNT").Value = request.RC_AMOUNT;
                transactions_lines[transactions_lines.Count - 1].FieldByName("CURRSEL_TRANS").Value = request.CURRSEL_TRANS;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANK_PROC_TYPE").Value = request.BANK_PROC_TYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DATA_REFERENCE").Value = request.DataReference;
                transactions_lines[transactions_lines.Count - 1].FieldByName("AFFECT_RISK").Value = request.AFFECT_RISK;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BNK_CRE_SOURCE").Value = request.BNK_CRE_SOURCE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BNK_CRE_LINE_TYPE").Value = request.BNK_CRE_LINE_TYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BN_INT_GL_CODE").Value = request.BN_INT_GL_CODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BN_CRDTYPE").Value = request.BN_CRDTYPE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("DIVISION").Value = request.DIVISION;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BN_FIN_COST_GL_CODE").Value = request.BN_FIN_COST_GL_CODE;
                transactions_lines[transactions_lines.Count - 1].FieldByName("BANK_CREDIT_CODE").Value = request.KrediKodu;
                transactions_lines[transactions_lines.Count - 1].FieldByName("GUID").Value =
                    string.IsNullOrEmpty(request.GUID) ? Guid.NewGuid().ToString().ToUpper() : request.GUID;

                bankvo.DataFields.FieldByName("GUID").Value =
                    string.IsNullOrEmpty(request.GUID) ? Guid.NewGuid().ToString().ToUpper() : request.GUID;

                // BNCREPAYMENTLIST ekleme
                var bncrepaymentlist_lines = bankvo.DataFields.FieldByName("BNCREPAYMENTLIST").Lines;
                foreach (var payment in request.BnkCrePaymentList)
                {
                    bncrepaymentlist_lines.AppendLine();
                    int lastIndex = bncrepaymentlist_lines.Count - 1;

                    bncrepaymentlist_lines[lastIndex].FieldByName("PER_NR").Value = payment.PER_NR;
                    bncrepaymentlist_lines[lastIndex].FieldByName("TRANS_TYPE").Value = payment.TRANS_TYPE;
                    bncrepaymentlist_lines[lastIndex].FieldByName("PARENT_REF").Value = payment.PARENT_REF;
                    bncrepaymentlist_lines[lastIndex].FieldByName("DUE_DATE").Value = payment.DUE_DATE.ToString("dd.MM.yyyy");
                    bncrepaymentlist_lines[lastIndex].FieldByName("OPR_DATE").Value = payment.OPR_DATE.ToString("dd.MM.yyyy");
                    bncrepaymentlist_lines[lastIndex].FieldByName("LINE_NR").Value = payment.LINE_NR;
                    bncrepaymentlist_lines[lastIndex].FieldByName("TOTAL").Value = payment.TOTAL;
                    bncrepaymentlist_lines[lastIndex].FieldByName("INT_TOTAL").Value = payment.INT_TOTAL;
                    bncrepaymentlist_lines[lastIndex].FieldByName("BANK_FICHE_REF").Value = payment.BANK_FICHE_REF;
                    bncrepaymentlist_lines[lastIndex].FieldByName("TR_RATE_CR").Value = payment.TR_RATE_CR;
                    bncrepaymentlist_lines[lastIndex].FieldByName("TR_RATE_ACC").Value = payment.TR_RATE_ACC;
                    bncrepaymentlist_lines[lastIndex].FieldByName("CREATED_BY").Value = payment.CREATED_BY;
                    bncrepaymentlist_lines[lastIndex].FieldByName("DATE_CREATED").Value = payment.DATE_CREATED.ToString("dd.MM.yyyy");
                    bncrepaymentlist_lines[lastIndex].FieldByName("HOUR_CREATED").Value = payment.HOUR_CREATED;
                    bncrepaymentlist_lines[lastIndex].FieldByName("MIN_CREATED").Value = payment.MIN_CREATED;
                    bncrepaymentlist_lines[lastIndex].FieldByName("SEC_CREATED").Value = payment.SEC_CREATED;
                    bncrepaymentlist_lines[lastIndex].FieldByName("LN_ACC_CODE").Value = payment.LN_ACC_CODE;
                }

                bool postResult = bankvo.Post();

                if (postResult)
                {
                    int fisRef = Convert.ToInt32(bankvo.DataFields.FieldByName("LOGICALREF").Value);

                    _logger.LogInformation("Kredi taksit ödeme fişi başarıyla oluşturuldu. FisRef: {FisRef}", fisRef);

                    var sonuc = new KrediTaksitSonuc
                    {
                        Basarili = true,
                        FisReferans = fisRef,
                        Mesaj = "Kredi taksit ödeme fişi başarıyla oluşturuldu.",
                        IslemTarihi = DateTime.Now,
                        IslemKodu = request.IslemKodu,
                        GUID = bankvo.DataFields.FieldByName("GUID").Value.ToString(),
                        OdenenAnaPara = request.AnaParaTutar,
                        OdenenFaiz = request.FaizTutar,
                        OdenenBsmv = request.BsmvTutar,
                        ToplamOdenen = toplamTutar,
                        TaksitNo = request.TaksitNo
                    };

                    return ServiceResult<KrediTaksitSonuc>.SuccessResult(sonuc);
                }
                else
                {
                    return HandleKrediTaksitErrors(bankvo, request.IslemKodu);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kredi taksit ödeme işleminde beklenmeyen hata: {IslemKodu}", request.IslemKodu);
                return ServiceResult<KrediTaksitSonuc>.ErrorResult($"Sistem hatası: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IslemDurumuSonuc>> IslemDurumuKontrolAsync(int fisReferans)
        {
            try
            {
                _logger.LogInformation("İşlem durumu kontrol ediliyor. FisRef: {FisRef}", fisReferans);

                // Burada Logo Tiger API'sinden durum sorgulanacak
                // Şimdilik başarılı döndürüyoruz

                var sonuc = new IslemDurumuSonuc
                {
                    Basarili = true,
                    FisReferans = fisReferans,
                    Mesaj = "İşlem başarılı durumda",
                    Durum = "TAMAMLANDI",
                    SorgulamaTarihi = DateTime.Now
                };

                return ServiceResult<IslemDurumuSonuc>.SuccessResult(sonuc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İşlem durumu kontrolünde hata: {FisRef}", fisReferans);
                return ServiceResult<IslemDurumuSonuc>.ErrorResult($"Durum kontrol hatası: {ex.Message}");
            }
        }

        #region Private Helper Methods
        private bool ValidateGelenHavaleRequest(GelenHavaleRequest request, out string error)
        {
            error = string.Empty;

            if (string.IsNullOrEmpty(request.IslemKodu))
            {
                error = "İşlem kodu boş olamaz";
                return false;
            }

            if (request.Tutar <= 0)
            {
                error = "Tutar sıfırdan büyük olmalıdır";
                return false;
            }

            if (string.IsNullOrEmpty(request.BankaHesapKodu))
            {
                error = "Banka hesap kodu boş olamaz";
                return false;
            }

            if (string.IsNullOrEmpty(request.FisNo))
            {
                error = "Fiş numarası boş olamaz";
                return false;
            }

            return true;
        }

        private bool ValidateGidenHavaleRequest(GidenHavaleRequest request, out string error)
        {
            error = string.Empty;

            if (string.IsNullOrEmpty(request.IslemKodu))
            {
                error = "İşlem kodu boş olamaz";
                return false;
            }

            if (request.Tutar <= 0)
            {
                error = "Tutar sıfırdan büyük olmalıdır";
                return false;
            }

            if (string.IsNullOrEmpty(request.BankaHesapKodu))
            {
                error = "Banka hesap kodu boş olamaz";
                return false;
            }

            if (string.IsNullOrEmpty(request.FisNo))
            {
                error = "Fiş numarası boş olamaz";
                return false;
            }

            return true;
        }

        private bool ValidateVirmanRequest(VirmanRequest request, out string error)
        {
            error = string.Empty;

            if (string.IsNullOrEmpty(request.IslemKodu))
            {
                error = "İşlem kodu boş olamaz";
                return false;
            }

            if (request.Tutar <= 0)
            {
                error = "Tutar sıfırdan büyük olmalıdır";
                return false;
            }

            if (string.IsNullOrEmpty(request.BankaHesapKodu))
            {
                error = "Banka hesap kodu boş olamaz";
                return false;
            }

            if (string.IsNullOrEmpty(request.HedefHesapKodu))
            {
                error = "Hedef hesap kodu boş olamaz";
                return false;
            }

            if (string.IsNullOrEmpty(request.FisNo))
            {
                error = "Fiş numarası boş olamaz";
                return false;
            }

            return true;
        }

        private bool ValidateKrediTaksitRequest(KrediTaksitRequest request, out string error)
        {
            error = string.Empty;

            if (string.IsNullOrEmpty(request.IslemKodu))
            {
                error = "İşlem kodu boş olamaz";
                return false;
            }

            if (request.ToplamTutar <= 0)
            {
                error = "Toplam tutar sıfırdan büyük olmalıdır";
                return false;
            }

            if (string.IsNullOrEmpty(request.BankaHesapKodu))
            {
                error = "Banka hesap kodu boş olamaz";
                return false;
            }

            if (string.IsNullOrEmpty(request.KrediHesapKodu))
            {
                error = "Kredi hesap kodu boş olamaz";
                return false;
            }

            if (string.IsNullOrEmpty(request.FisNo))
            {
                error = "Fiş numarası boş olamaz";
                return false;
            }

            return true;
        }

        private ServiceResult<T> HandleLogoErrors<T>(UnityObjects.Data bankvo, string islemKodu) where T : BankaFisSonuc, new()
        {
            List<string> hatalar = new List<string>();

            if (bankvo.ErrorCode != 0)
            {
                string error = $"DBError({bankvo.ErrorCode})-{bankvo.ErrorDesc}{bankvo.DBErrorDesc}";
                hatalar.Add(error);
                _logger.LogError("Logo Tiger DB Hatası: {Error}", error);
            }

            if (bankvo.ValidateErrors.Count > 0)
            {
                string result = "XML ErrorList:";
                for (int i = 0; i < bankvo.ValidateErrors.Count; i++)
                {
                    string hata = $"({bankvo.ValidateErrors[i].ID}) - {bankvo.ValidateErrors[i].Error}";
                    hatalar.Add(hata);
                    result += hata;
                }
                _logger.LogError("Logo Tiger Validasyon Hatası: {Result}", result);
            }

            var sonuc = new T
            {
                Basarili = false,
                FisReferans = 0,
                Mesaj = "Logo Tiger entegrasyon hatası",
                IslemTarihi = DateTime.Now,
                IslemKodu = islemKodu,
                Hatalar = hatalar,
                GUID = bankvo.DataFields.FieldByName("GUID")?.Value?.ToString() ?? string.Empty
            };

            return ServiceResult<T>.ErrorResult("Logo Tiger entegrasyon hatası", hatalar);
        }

        private ServiceResult<KrediTaksitSonuc> HandleKrediTaksitErrors(UnityObjects.Data bankvo, string islemKodu)
        {
            var result = HandleLogoErrors<KrediTaksitSonuc>(bankvo, islemKodu);

            if (!result.Success)
            {
                result.Data.OdenenAnaPara = 0;
                result.Data.OdenenFaiz = 0;
                result.Data.OdenenBsmv = 0;
                result.Data.ToplamOdenen = 0;
                result.Data.TaksitNo = 0;
            }

            return result;
        }
        #endregion
    }

    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime ResponseTime { get; set; } = DateTime.UtcNow;

        public static ServiceResult<T> SuccessResult(T data, string message = null)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data,
                Message = message ?? "İşlem başarıyla tamamlandı",
                Errors = new List<string>()
            };
        }

        public static ServiceResult<T> ErrorResult(string message, List<string> errors = null)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Data = default,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}