using System.Globalization;
using TurkSoft.Business.Base;
using TurkSoft.Business.Interface;
using TurkSoft.Business.Managers.BankProviders.Infrastructure;
using TurkSoft.Entities.BankService.Contracts;
using TurkSoft.Entities.BankService.Models;

using EmlakSrv; // svcutil ile üretilen namespace

namespace TurkSoft.Business.Managers.BankProviders
{
    public sealed class EmlakbankStatementProvider : IBankStatementProvider
    {
        public int BankId => BankIds.Emlakbank;
        public string BankCode => "EML";

        public async Task<IReadOnlyList<BNKHAR>> GetStatementAsync(BankStatementRequest request, CancellationToken ct = default)
        {
            // 1) Account parse (123456-1)
            var (accNo, accSuffix) = ParseAccount(request);

            // 2) WCF Request oluştur
            var wcfReq = new AccountStatetmentRequest
            {
                ExtUName = request.Username,
                ExtUPassword = request.Password,

                AccountNumber = accNo,
                AccountSuffix = accSuffix,

                BeginDate = request.BeginDate,
                EndDate = request.EndDate,

                // Opsiyonel alanlar (bankadan zorunlu gelirse extras ile override edersin)
                AppName = request.GetExtra("appName") ?? "TurkSoft.BankService",
                DeviceId = request.GetExtra("deviceId") ?? Environment.MachineName,
                DeviceModel = request.GetExtra("deviceModel") ?? "Server",
                DeviceOSName = request.GetExtra("deviceOSName") ?? "Windows",
                DeviceOSVersion = request.GetExtra("deviceOSVersion") ?? Environment.OSVersion.VersionString,
                Version = request.GetExtra("version") ?? "1.0",
                MethodName = request.GetExtra("methodName") ?? "GetAccountStatement",
                MainResourceCode = request.GetExtra("mainResourceCode"),
            };

            if (int.TryParse(request.GetExtra("languageId"), out var langId))
                wcfReq.LanguageId = langId;

            if (int.TryParse(request.GetExtra("mainResourceId"), out var mrid))
                wcfReq.MainResourceId = mrid;

            // 3) Client oluştur (Link ile override edilebilir)
            // DİKKAT: WSDL değil, servis endpoint (…/Basic) verilmelidir.
            // Default zaten proxy içinde: https://.../AccountStatementService.svc/Basic
            var endpoint = string.IsNullOrWhiteSpace(request.Link)
                ? null
                : request.Link;

            var client = endpoint is null
                ? new AccountStatementServiceClient(AccountStatementServiceClient.EndpointConfiguration.BasicHttpBinding_IAccountStatementService)
                : new AccountStatementServiceClient(AccountStatementServiceClient.EndpointConfiguration.BasicHttpBinding_IAccountStatementService, endpoint);

            try
            {
                var resp = await client.GetAccountStatementAsync(wcfReq);

                if (resp == null)
                    throw new Exception("Emlakbank: Boş yanıt döndü.");

                if (!resp.Success)
                {
                    var msg = resp.ErrorMessage ?? (resp.Results != null && resp.Results.Length > 0
                        ? string.Join(" | ", resp.Results.Select(r => $"{r.ErrorCode}:{r.ErrorMessage}"))
                        : "Bilinmeyen hata");
                    throw new Exception($"Emlakbank: Success=false. {msg}");
                }

                if (!string.IsNullOrWhiteSpace(resp.ErrorCode))
                    throw new Exception($"Emlakbank: {resp.ErrorCode} - {resp.ErrorMessage}");

                // 4) Map -> BNKHAR
                var list = new List<BNKHAR>();
                var accounts = resp.Value ?? Array.Empty<AccountContract>();

                foreach (var acc in accounts)
                {
                    var details = acc.Details ?? Array.Empty<TransactionDetailContract>();

                    foreach (var d in details)
                    {
                        // debit/credit kararını basit tuttum:
                        // Credit > 0 => Alacak (A), aksi => Borç (B)
                        var debOrCred = d.Credit > 0 ? "A" : "B";

                        var processId = !string.IsNullOrWhiteSpace(d.TranRef)
                            ? d.TranRef
                            : (d.BusinessKey?.ToString(CultureInfo.InvariantCulture) ?? Guid.NewGuid().ToString("N"));

                        list.Add(new BNKHAR
                        {
                            BNKCODE = BankCode,
                            HESAPNO = $"{acc.AccountNumber}-{acc.AccountSuffix}",

                            URF = request.GetExtra("customerNo") ?? "", // bankanın müşteri no’su elinde varsa extras ile ver
                            SUBECODE = acc.BranchId.ToString(CultureInfo.InvariantCulture),
                            CURRENCYCODE = acc.FECName, // ya da acc.FEC / FECLongName; bankanın döndürdüğüne göre

                            PROCESSID = processId,
                            PROCESSREFNO = d.TranRef ?? "",
                            PROCESSTIMESTR = d.TranDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            PROCESSTIMESTR2 = (d.ValueDate ?? d.TranDate).ToString("yyyy-MM-ddTHH:mm:ss"),
                            PROCESSTIME = d.TranDate,
                            PROCESSTIME2 = d.ValueDate ?? d.TranDate,

                            PROCESSAMAOUNT = d.Amount.ToString(CultureInfo.InvariantCulture),
                            PROCESSBALANCE = d.CurrentBalance.ToString(CultureInfo.InvariantCulture),

                            PROCESSDESC = d.Description ?? "",
                            PROCESSDESC2 = d.TranType ?? "",
                            PROCESSDESC3 = d.ResourceCode ?? "",

                            PROCESSIBAN = acc.IBAN ?? "",
                            PROCESSDEBORCRED = debOrCred,

                            PROCESSTYPECODE = d.TranType ?? "",
                            PROCESSTYPECODEMT940 = null,

                            // karşı taraf (varsa)
                            FRMIBAN = d.SenderIBAN ?? "",
                            PROCESSVKN = d.SenderIdentityNumber ?? "",

                            Durum = 0
                        });
                    }
                }

                return list;
            }
            finally
            {
                client.SafeClose(); // senin extensionın
            }
        }

        private static (int accNo, short suffix) ParseAccount(BankStatementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                throw new ArgumentException("Emlakbank için AccountNumber zorunlu.");

            var raw = request.AccountNumber.Trim();

            // "123456-1"
            if (raw.Contains('-'))
            {
                var parts = raw.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length >= 1 && int.TryParse(parts[0], out var n))
                {
                    short sfx = 0;
                    if (parts.Length >= 2) short.TryParse(parts[1], out sfx);
                    return (n, sfx);
                }
            }

            // sadece "123456"
            if (int.TryParse(raw, out var only))
            {
                // extras["suffix"] varsa onu kullan
                if (short.TryParse(request.GetExtra("suffix"), out var sfx))
                    return (only, sfx);

                return (only, 0);
            }

            throw new ArgumentException($"Emlakbank AccountNumber formatı hatalı: '{request.AccountNumber}'. Örn: '123456-1'");
        }
    }
}
