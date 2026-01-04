using System.Globalization;
using TurkSoft.Business.Base;
using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers.BankProviders.Infrastructure;
using TurkSoft.Entities.BankService.Contracts;
using TurkSoft.Entities.BankService.Models;
using VakıfKatilimSrv;

namespace TurkSoft.Business.Managers.BankProviders
{
    public sealed class VakifKatilimStatementProvider : IBankStatementProvider
    {
        public int BankId => BankIds.VakifKatilim;
        public string BankCode => "VKT";

        public async Task<IReadOnlyList<BNKHAR>> GetStatementAsync(BankStatementRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("Vakıf Katılım için Username zorunlu.");
            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Vakıf Katılım için Password zorunlu.");
            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                throw new ArgumentException("Vakıf Katılım için AccountNumber zorunlu.");

            // AccountNumber parse: "1234567-1" -> (no=1234567, suffix=1)
            var (accNo, accSuffix) = ParseAccountNoSuffix(request);

            // Debit/Credit filtre (opsiyonel)
            var dcc = ParseDebitCreditCode(request.GetExtra("debitCreditCode")); // All/Debit/Credit

            // Endpoint override: WSDL değil, /Basic endpoint kullan
            // Default zaten proxy içinde: .../CustomerTransactionService.svc/Basic
            var client = string.IsNullOrWhiteSpace(request.Link)
                ? new CustomerTransactionServiceClient(CustomerTransactionServiceClient.EndpointConfiguration.BasicHttpBinding_ICustomerTransactionService)
                : new CustomerTransactionServiceClient(
                    CustomerTransactionServiceClient.EndpointConfiguration.BasicHttpBinding_ICustomerTransactionService,
                    request.Link
                  );

            try
            {
                ct.ThrowIfCancellationRequested();

                var req = new CustomerTransactionRequest
                {
                    // Auth
                    ExtUName = request.Username,
                    ExtUPassword = request.Password,

                    // Zorunlu alanlar
                    AccountNumber = accNo,
                    AccountSuffix = accSuffix,
                    BeginDate = request.BeginDate,
                    EndDate = request.EndDate,

                    DebitCreditCode = dcc,
                    HasTimeFilter = false,
                    SlipBusinessKey = 0m,
                    TransactionType = null,

                    // opsiyonel
                    MethodName = "GetCustomerTransactionDetails",
                    IsNewDefinedTransaction = false,
                    LanguageId = request.GetExtra("languageId") is string lid && int.TryParse(lid, out var li) ? li : null
                };

                // Daha detaylı bilgi verdiği için Details kullanıyoruz:
                var resp = await client.GetCustomerTransactionDetailsAsync(req);

                if (resp == null)
                    throw new Exception("Vakıf Katılım: Boş yanıt döndü.");

                // BOA servislerinde bazen Success + ErrorCode/ErrorMessage birlikte bulunur
                if (!resp.Success || (!string.IsNullOrWhiteSpace(resp.ErrorCode) && resp.ErrorCode != "0"))
                    throw new Exception($"Vakıf Katılım: {resp.ErrorCode} - {resp.ErrorMessage}");

                var rows = resp.Value ?? Array.Empty<CustomerTransactionDetailResponseModel>();
                var list = new List<BNKHAR>(rows.Length);

                foreach (var x in rows)
                {
                    ct.ThrowIfCancellationRequested();

                    var pid = x.BusinessKey.ToString(CultureInfo.InvariantCulture);

                    // Debit/Credit tahmini (Amount işaretinden). Eğer banka her zaman + gönderiyorsa boş bırakmak daha doğru olur.
                    var debCred = x.Amount < 0 ? "B" : "A";

                    var desc = FirstNonEmpty(x.Description, x.Comment, x.TransactionTypeDescription) ?? "";

                    var counterParty =
                        FirstNonEmpty(x.ReceiverName, x.SenderName) ??
                        FirstNonEmpty(x.ReceiverAccountNumber, x.SenderAccountNumber) ??
                        "";

                    var vkn =
                        FirstNonEmpty(x.ReceiverTaxNumber, x.SenderTaxNumber) ?? "";

                    list.Add(new BNKHAR
                    {
                        BNKCODE = BankCode,

                        // Bizde accountNumber string idi; aynen taşımak daha sağlıklı
                        HESAPNO = request.AccountNumber,

                        // BOA response müşteri no dönmüyor; istersen Extras ile al:
                        URF = request.GetExtra("customerNo") ?? "",
                        SUBECODE = x.TranBranchId.ToString(CultureInfo.InvariantCulture),

                        PROCESSID = pid,
                        PROCESSREFNO = FirstNonEmpty(x.QueryTokenStr, x.QueryToken?.ToString()) ?? pid,

                        PROCESSTIME = x.TranDate,
                        PROCESSTIME2 = x.SystemDate,
                        PROCESSTIMESTR = x.TranDate.ToString("O"),
                        PROCESSTIMESTR2 = x.SystemDate.ToString("O"),

                        PROCESSAMAOUNT = x.Amount.ToString(CultureInfo.InvariantCulture),
                        PROCESSBALANCE = (x.Balance1 ?? x.Balance)?.ToString(CultureInfo.InvariantCulture) ?? "0",

                        PROCESSDESC = desc,
                        PROCESSDESC2 = x.TransactionTypeDescription ?? "",
                        PROCESSDESC3 = counterParty,

                        PROCESSVKN = vkn,

                        // karşı hesap bilgisi (IBAN yok, hesap no var)
                        PROCESSIBAN = FirstNonEmpty(x.ReceiverAccountNumber, x.SenderAccountNumber) ?? "",

                        PROCESSDEBORCRED = debCred, // "A" / "B"
                        PROCESSTYPECODE = x.TransactionType?.ToString() ?? "",
                        PROCESSTYPECODEMT940 = null,

                        Durum = 0
                    });
                }

                return list;
            }
            finally
            {
                client.SafeClose();
            }
        }

        private static (int accountNo, short suffix) ParseAccountNoSuffix(BankStatementRequest request)
        {
            // 1) "1234567-1"
            var s = request.AccountNumber.Trim();

            if (s.Contains('-'))
            {
                var parts = s.Split('-', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out var no1) &&
                    short.TryParse(parts[1], out var su1))
                    return (no1, su1);
            }

            // 2) sadece "1234567" + extras["accountSuffix"]
            if (!int.TryParse(s, out var no))
                throw new ArgumentException($"Vakıf Katılım: AccountNumber int olmalı (gelen: {request.AccountNumber}). " +
                                            $"Eğer format 'no-suffix' ise örn '1234567-1' gönder.");

            var suffixStr = request.GetExtra("accountSuffix") ?? request.GetExtra("suffix") ?? "0";
            if (!short.TryParse(suffixStr, out var su))
                su = 0;

            return (no, su);
        }

        private static DebitCreditCode ParseDebitCreditCode(string? v)
        {
            if (string.IsNullOrWhiteSpace(v)) return DebitCreditCode.All;
            return v.Trim().ToLowerInvariant() switch
            {
                "debit" or "borc" or "b" => DebitCreditCode.Debit,
                "credit" or "alacak" or "a" => DebitCreditCode.Credit,
                _ => DebitCreditCode.All
            };
        }

        private static string? FirstNonEmpty(params string?[] vals)
            => vals.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
    }
}
