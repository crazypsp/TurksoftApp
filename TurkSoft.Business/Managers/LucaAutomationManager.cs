using Microsoft.Playwright;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using TurkSoft.Business.Interface;
using TurkSoft.Core.Result.Class;
using TurkSoft.Core.Result.Interface;
using TurkSoft.Entities.Luca;

namespace TurkSoft.Business.Managers
{
    /// <summary>
    /// Luca otomasyon işlemlerini yöneten sınıf.
    /// Playwright ile giriş, şirket listesi/şirket seçimi, fiş gönderimi ve hesap planı okuma.
    /// </summary>
    public class LucaAutomationManager : ILucaAutomationBussiness
    {
        private const int CaptchaPollDelay = 5000;
        private const int LucaMMPollDelay = 10000;
        private const int LucaLoginPollDelay = 2000;

        private IBrowser? _browser;
        private IBrowserContext? _context;
        private IPage? _popup;

        #region LOGIN

        public async Task<IResult> LoginAsync(LucaLoginRequest user)
        {
            try
            {
                var playwright = await Microsoft.Playwright.Playwright.CreateAsync();

                _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Channel = "chrome",
                    Headless = false,
                    Args = new[]
                    {
                        "--headless=new",
                        "--disable-blink-features=AutomationControlled",
                        "--disable-features=BlockThirdPartyCookies,SameSiteByDefaultCookies,CookiesWithoutSameSiteMustBeSecure",
                        "--no-first-run",
                        "--no-default-browser-check",
                        "--window-size=1920,1080"
                    }
                });

                _context = await _browser.NewContextAsync(new BrowserNewContextOptions
                {
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36",
                    Locale = "tr-TR",
                    TimezoneId = "Europe/Istanbul",
                    ViewportSize = null,
                    BypassCSP = true
                });

                await _context.AddInitScriptAsync("Object.defineProperty(navigator, 'webdriver', { get: () => undefined });");
                _context.SetDefaultTimeout(60000);
                _context.SetDefaultNavigationTimeout(60000);

                var mainPage = await _context.NewPageAsync();
                await mainPage.GotoAsync("https://luca.com.tr/");

                await mainPage.ClickAsync("a.dropdown-toggle:has-text('Sistem')");
                _popup = await mainPage.RunAndWaitForPopupAsync(() => mainPage.ClickAsync("a[onclick*=popup]"));

                // Kimlik bilgileri
                await Task.Delay(LucaLoginPollDelay);
                await _popup.FillAsync("#musteriNo", user.CustumerNo);
                await Task.Delay(LucaLoginPollDelay);
                await _popup.FillAsync("#kullaniciAdi", user.UserName);
                await Task.Delay(LucaLoginPollDelay);
                await _popup.FillAsync("#parola", user.Password);
                await Task.Delay(LucaLoginPollDelay);
                await _popup.ClickAsync("input[value='GİRİŞ']");
                await Task.Delay(LucaLoginPollDelay);
                await _popup.WaitForTimeoutAsync(await GetSmartDelayAsync());

                // CAPTCHA varsa çöz
                var captchaElement = _popup.Locator("div.captcha img");
                if (await captchaElement.IsVisibleAsync())
                    await HandleCaptchaAsync(user, captchaElement);

                // SSO → MMP’ye geçiş
                var popupRace = _popup.WaitForPopupAsync();
                var navRace = _popup.WaitForNavigationAsync(new PageWaitForNavigationOptions { WaitUntil = WaitUntilState.Commit });

                await _popup.ClickAsync(".lucaMmpLogo");
                await Task.Delay(LucaMMPollDelay);

                IPage? mmpPage = null;
                try { mmpPage = await popupRace; } catch { /* aynı sekmede açılabilir */ }
                if (mmpPage == null)
                {
                    try { await navRace; } catch { }
                    mmpPage = _popup; // aynı pencerede
                }

                var ok = await SpinWaitUrlNotContainsAsync(mmpPage!, "ssoGiris.do", timeoutMs: 90000);
                if (!ok)
                    throw new TimeoutException("SSO yönlendirmesi tamamlanamadı (ssoGiris.do üzerinde kaldı).");

                await mmpPage!.Locator("#SirketCombo, frame[name='frm4']").First
                              .WaitForAsync(new LocatorWaitForOptions { Timeout = 60000 });

                LucaSession.MmpPage = mmpPage;
                return new SuccessResult("Luca sistemine başarıyla giriş yapıldı.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Giriş hatası: {ex.Message}");
            }
        }

        #endregion

        #region COMPANY (List/Select)

        public async Task<IDataResult<List<CompanyCode>>> GetCompanyAsync()
        {
            try
            {
                var mmp = LucaSession.MmpPage ?? throw new InvalidOperationException("Aktif oturum bulunamadı. Önce login olun.");

                var combo = await FindVisibleAsync(mmp, "#SirketCombo", timeoutMs: 6000);
                if (combo is null)
                    return new DataResult<List<CompanyCode>>(null, false, "#SirketCombo bulunamadı.");

                var options = combo.Locator("option");
                int count = await options.CountAsync();

                var list = new List<CompanyCode>(Math.Max(0, count - 1));
                for (int i = 0; i < count; i++)
                {
                    var opt = options.Nth(i);
                    var val = (await opt.GetAttributeAsync("value"))?.Trim() ?? "";
                    var txt = (await opt.InnerTextAsync())?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(val)) continue;
                    list.Add(new CompanyCode { Values = val, Name = txt });
                }

                return new DataResult<List<CompanyCode>>(list, true, "Şirket listesi alındı.");
            }
            catch (Exception ex)
            {
                return new DataResult<List<CompanyCode>>(null, false, $"Şirket listesi hatası: {ex.Message}");
            }
        }

        public async Task<IResult> SelectCompanyAsync(string companyCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(companyCode))
                    return new ErrorResult("Şirket kodu zorunludur.");

                var mmp = LucaSession.MmpPage ?? throw new InvalidOperationException("Aktif oturum bulunamadı. Önce login olun.");

                var combo = await FindVisibleAsync(mmp, "#SirketCombo", timeoutMs: 6000);
                if (combo is null)
                    return new ErrorResult("#SirketCombo bulunamadı.");

                await combo.SelectOptionAsync(new[] { companyCode });

                // Alt barda SirName → 'Tamam' varsa tıkla
                await ClickSirNameTamamIfActiveAsync(mmp, timeoutMs: 10000);

                // Doğrula
                string selected = string.Empty;
                try { selected = await combo.InputValueAsync(); }
                catch
                {
                    combo = await FindVisibleAsync(mmp, "#SirketCombo", timeoutMs: 4000);
                    if (combo != null) selected = await combo.InputValueAsync();
                }

                if (!string.Equals(selected, companyCode, StringComparison.OrdinalIgnoreCase))
                {
                    await Task.Delay(600);
                    if (combo is null) combo = await FindVisibleAsync(mmp, "#SirketCombo", timeoutMs: 4000);
                    if (combo != null) selected = await combo.InputValueAsync();
                }

                if (!string.Equals(selected, companyCode, StringComparison.OrdinalIgnoreCase))
                    return new ErrorResult("Şirket seçimi doğrulanamadı.");

                // Firma değişti; eski cache/frame kullanılmasın
                LucaSession.CachedHesapPlani = null;
                LucaSession.Frame = null;

                return new SuccessResult("Şirket seçildi ve onaylandı.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Şirket seçimi hatası: {ex.Message}");
            }
        }

        #endregion

        #region ACCOUNTING PLAN

        public async Task<IDataResult<List<AccountingCode>>> GetAccountingPlanAsync()
        {
            try
            {
                var mmp = LucaSession.MmpPage ?? throw new InvalidOperationException("Aktif oturum bulunamadı.");

                // SirName/Tamam barı açıksa kapat
                await ClickSirNameTamamIfActiveAsync(mmp, 4000);

                // Fiş ekranını her çağrıda taze aç
                var fisFrame = await OpenFisScreenAsync(mmp);
                if (fisFrame is null)
                    return new DataResult<List<AccountingCode>>(null, false, "Fiş ekranı açılamadı.");

                // Hesap planı listesi görünür değilse F9 ile getir
                await EnsureHesapPlaniListVisibleAsync(fisFrame);

                // DOM’dan tüm satırları tek seferde JSON string olarak al
                var json = await fisFrame.EvaluateAsync<string>(
                @"() => {
                    const out = [];
                    const tbody = document.querySelector('table#hsptable tbody#hstbody');
                    if (!tbody) return '[]';
                    const rows = tbody.querySelectorAll('tr');
                    rows.forEach(tr => {
                      const tds = tr.querySelectorAll('td');
                      const code = (tds[0]?.textContent || '').trim();
                      const name = (tds[1]?.textContent || '').trim();
                      if (code && name) out.push({ code, name });
                    });
                    return JSON.stringify(out);
                }");

                var tmp = JsonSerializer.Deserialize<List<_JsAcc>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<_JsAcc>();

                var list = tmp
                    .Where(x => !string.IsNullOrWhiteSpace(x.code) && !string.IsNullOrWhiteSpace(x.name))
                    .Select(x => new AccountingCode { Code = x.code!.Trim(), Name = x.name!.Trim() })
                    .ToList();

                LucaSession.Frame = fisFrame;
                LucaSession.CachedHesapPlani = list;

                return new DataResult<List<AccountingCode>>(list, true, $"Hesap planı başarıyla alındı. (satır: {list.Count})");
            }
            catch (TimeoutException tex)
            {
                return new DataResult<List<AccountingCode>>(null, false, $"Plan çekme hatası (timeout): {tex.Message}");
            }
            catch (Exception ex)
            {
                return new DataResult<List<AccountingCode>>(null, false, $"Plan çekme hatası: {ex.Message}");
            }
        }

        private sealed class _JsAcc
        {
            public string? code { get; set; }
            public string? name { get; set; }
        }

        #endregion

        #region SEND FIS

        public async Task<IResult> SendFisRowsAsync(List<LucaFisRow> rows)
        {
            try
            {
                var fisFrame = LucaSession.Frame ?? throw new InvalidOperationException("Fiş ekranı bulunamadı. Önce hesap planı akışını çalıştırın.");
                const string tableSelector = "table#TBL";
                bool newFisStart = true;

                const int BATCH_SIZE = 400;
                int rowsInCurrentFis = 0;

                static async Task WaitForAfterSaveAsync(IFrame frame, string tblSel)
                {
                    // SweetAlert kapat
                    var alert = frame.Locator("div.swal2-container");
                    if (await alert.IsVisibleAsync(new() { Timeout = 1000 }))
                    {
                        var ok = alert.Locator("button.swal2-confirm");
                        if (await ok.IsVisibleAsync(new() { Timeout = 1000 }))
                            await ok.ClickAsync();
                        await alert.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
                    }

                    // Son satır input tekrar etkin olsun
                    var hesap = frame.Locator($"{tblSel} tr:last-child input[name='HESAP_KODU']");
                    await hesap.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 60000 });

                    await frame.WaitForFunctionAsync(
                        @"sel => { 
                            const el = document.querySelector(sel); 
                            return !!el && !el.disabled && !el.readOnly; 
                          }",
                        $"{tblSel} tr:last-child input[name='HESAP_KODU']",
                        new() { Timeout = 60000 });
                }

                for (int i = 0; i < rows.Count; i++)
                {
                    // Yeni satır ekle
                    if (!newFisStart)
                    {
                        await fisFrame.ClickAsync($"{tableSelector} tr:last-child td.add_delete.btn-td input[value='+']");
                        await fisFrame.WaitForSelectorAsync($"{tableSelector} tr:last-child input[name='HESAP_KODU']");
                    }
                    newFisStart = false;

                    var currentRowSelector = $"{tableSelector} tr:last-child";
                    var r = rows[i];

                    if (string.IsNullOrWhiteSpace(r.HesapKodu))
                        throw new InvalidOperationException($"[{i + 1}. satır] Hesap Kodu boş olamaz.");

                    var hesapInput = fisFrame.Locator($"{currentRowSelector} input[name='HESAP_KODU']");
                    await hesapInput.ClickAsync();
                    await hesapInput.FillAsync(r.HesapKodu.Trim());
                    await hesapInput.PressAsync("Tab");

                    await fisFrame.WaitForTimeoutAsync(120);
                    var typed = (await hesapInput.InputValueAsync())?.Trim();

                    if (string.IsNullOrEmpty(typed))
                    {
                        // F9 fallback
                        await hesapInput.ClickAsync();
                        await hesapInput.PressAsync("F9");

                        await fisFrame.WaitForSelectorAsync("table#hsptable");
                        var rowInList = fisFrame.Locator("table#hsptable tr").Filter(new() { HasText = r.HesapKodu.Trim() });
                        await rowInList.First.DblClickAsync();

                        await fisFrame.WaitForTimeoutAsync(120);
                        typed = (await hesapInput.InputValueAsync())?.Trim();
                        if (string.IsNullOrEmpty(typed))
                            throw new Exception($"[{i + 1}. satır] Hesap Kodu '{r.HesapKodu}' alanına yazılamadı.");
                    }

                    await fisFrame.FillAsync($"{currentRowSelector} input[name='EVRAK_NO']", r.EvrakNo);
                    await fisFrame.FillAsync($"{currentRowSelector} input[name='EVRAK_TARIHI']",
                        r.Tarih.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("tr-TR")));
                    await fisFrame.FillAsync($"{currentRowSelector} input[name='BELGE_TUR_KOD']", "MK");
                    await fisFrame.FillAsync($"{currentRowSelector} input[name='ACIKLAMA']", r.Aciklama);
                    await fisFrame.FillAsync($"{currentRowSelector} input[name='BORC']", ToMoney(r.Borc));
                    await fisFrame.FillAsync($"{currentRowSelector} input[name='ALACAK']", ToMoney(r.Alacak));
                    await fisFrame.FillAsync($"{currentRowSelector} input[name='MIKTAR']", "0,00000");

                    rowsInCurrentFis++;

                    if (rowsInCurrentFis >= BATCH_SIZE)
                    {
                        await fisFrame.ClickAsync("button#kaydetHref");
                        await WaitForAfterSaveAsync(fisFrame, tableSelector);

                        await fisFrame.ClickAsync("button#yeniHref");

                        var sw = Stopwatch.StartNew();
                        IFrame? newFrame = null;
                        while (sw.ElapsedMilliseconds < 15000)
                        {
                            var frames = LucaSession.MmpPage!.Context.Pages.Last().Frames;
                            newFrame = frames.FirstOrDefault(f => f.Url.Contains("addFis.do"));
                            if (newFrame != null)
                            {
                                var ok = await newFrame.Locator("table#TBL tr:last-child input[name='HESAP_KODU']")
                                                       .IsVisibleAsync(new() { Timeout = 2000 });
                                if (ok) break;
                            }
                            await Task.Delay(300);
                        }
                        if (newFrame == null)
                            throw new TimeoutException("Yeni fiş ekranı yüklenemedi.");

                        fisFrame = newFrame;
                        LucaSession.Frame = fisFrame;

                        newFisStart = true;
                        rowsInCurrentFis = 0;

                        await fisFrame.ClickAsync("button#kaydetHref");
                        await WaitForAfterSaveAsync(fisFrame, tableSelector);
                    }
                }

                await fisFrame.ClickAsync("button#kaydetHref");
                await WaitForAfterSaveAsync(fisFrame, tableSelector);

                await LucaSession.MmpPage!.Context.CloseAsync();
                return new SuccessResult("Fiş satırları başarıyla gönderildi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Fiş gönderme hatası: {ex.Message}");
            }
        }

        #endregion

        #region HELPERS (Captcha / Waits / UI / Navigation)

        private async Task HandleCaptchaAsync(LucaLoginRequest user, ILocator captchaElement)
        {
            bool solved = false;
            while (!solved)
            {
                var path = Path.Combine(Path.GetTempPath(), "captcha.png");
                await captchaElement.ScreenshotAsync(new LocatorScreenshotOptions { Path = path });
                var text = await SolveCaptchaAsync(path, user.ApiKey);

                await _popup!.FillAsync("#captcha-input", text);
                await _popup.ClickAsync("input[value='Tamam']");
                await _popup.WaitForTimeoutAsync(await GetSmartDelayAsync());

                var error = _popup.Locator("div.swal2-container");
                if (await error.IsVisibleAsync())
                {
                    var msg = await error.InnerTextAsync();
                    if (msg.Contains("doğru olduğundan emin olun."))
                    {
                        await error.Locator("button.swal2-confirm").ClickAsync();
                        continue;
                    }
                }
                solved = true;
            }
        }

        private async Task<string> SolveCaptchaAsync(string imagePath, string apiKey)
        {
            var bytes = await File.ReadAllBytesAsync(imagePath);
            var base64 = Convert.ToBase64String(bytes);

            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("key", apiKey),
                new KeyValuePair<string, string>("method", "base64"),
                new KeyValuePair<string, string>("body", base64),
                new KeyValuePair<string, string>("json", "1")
            });

            var response = await client.PostAsync("http://2captcha.com/in.php", content);
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
            string id = obj["request"].GetString()!;

            while (true)
            {
                await Task.Delay(CaptchaPollDelay);
                var check = await client.GetStringAsync($"http://2captcha.com/res.php?key={apiKey}&action=get&id={id}&json=1");
                var res = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(check)!;
                if (res["status"].GetInt32() == 1)
                    return res["request"].GetString()!;
            }
        }

        private static string ToMoney(decimal v)
            => v.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"));

        private static async Task<int> GetSmartDelayAsync()
        {
            try
            {
                var http = new HttpClient();
                var sw = Stopwatch.StartNew();
                await http.GetAsync("https://www.luca.com.tr/favicon.ico");
                sw.Stop();
                return (int)Math.Clamp(sw.Elapsed.TotalMilliseconds * 1.2, 300, 3000);
            }
            catch
            {
                return 1000;
            }
        }

        private static async Task<bool> SpinWaitUrlNotContainsAsync(IPage page, string token, int timeoutMs, int intervalMs = 400)
        {
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    var last = page.Context.Pages.LastOrDefault();
                    if (last != null && !ReferenceEquals(last, page))
                        page = last;

                    var url = page.Url ?? string.Empty;
                    if (!url.Contains(token, StringComparison.OrdinalIgnoreCase) &&
                        url != "about:blank" && url.Length > 0)
                        return true;
                }
                catch { }

                try { await page.EvaluateAsync("() => 0"); } catch { }
                await Task.Delay(intervalMs);
            }
            return false;
        }

        private static async Task<ILocator?> FindVisibleAsync(IPage page, string selector, int timeoutMs = 3000)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                foreach (var f in page.Frames)
                {
                    var loc = f.Locator(selector);
                    try
                    {
                        if (await loc.IsVisibleAsync(new() { Timeout = 500 }))
                            return loc;
                    }
                    catch { }
                }
                await Task.Delay(150);
            }
            return null;
        }

        /// <summary> Şirket seçiminden sonra sayfanın altındaki SirName alanında "Tamam" butonu görünürse tıklar. </summary>
        private static async Task ClickSirNameTamamIfActiveAsync(IPage page, int timeoutMs = 10000)
        {
            string[] candidates =
            {
                "#SirName button.green:has-text('Tamam')",
                "#SirName button:has-text('Tamam')",
                "button.no-bold.green:has-text('Tamam')",
                "button[onclick*='formSubmit']:has-text('Tamam')"
            };

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                foreach (var f in page.Frames)
                {
                    foreach (var sel in candidates)
                    {
                        var btn = f.Locator(sel);
                        try
                        {
                            if (await btn.IsVisibleAsync(new() { Timeout = 200 }))
                            {
                                await btn.ClickAsync();

                                try
                                {
                                    await f.Locator("#SirName").WaitForAsync(new()
                                    {
                                        State = WaitForSelectorState.Detached,
                                        Timeout = 5000
                                    });
                                }
                                catch { /* bazı temalarda #SirName DOM'da kalabilir */ }

                                return;
                            }
                        }
                        catch { }
                    }
                }
                await Task.Delay(200);
            }
            // Görünmezse sorun değil
        }

        /// <summary>
        /// Menü akışını takip ederek Fiş Girişi ekranını (addFis.do) açar ve tabloyu init eder.
        /// </summary>
        private static async Task<IFrame?> OpenFisScreenAsync(IPage mmp)
        {
            // Menü ve müşteri frame’leri
            var frames = mmp.Context.Pages.Last().Frames;
            var menuFrame = frames.FirstOrDefault(f => f.Url.Contains("menu.do") || f.Name == "frm2");
            var musteriFrame = frames.FirstOrDefault(f => f.Url.Contains("musteriBilgileri.do") || f.Name == "frm3");

            if (menuFrame == null || musteriFrame == null)
            {
                // Kısa bir deneme daha
                await Task.Delay(800);
                frames = mmp.Context.Pages.Last().Frames;
                menuFrame = frames.FirstOrDefault(f => f.Url.Contains("menu.do") || f.Name == "frm2");
                musteriFrame = frames.FirstOrDefault(f => f.Url.Contains("musteriBilgileri.do") || f.Name == "frm3");
                if (menuFrame == null || musteriFrame == null) return null;
            }

            await menuFrame.HoverAsync("font:has-text('Muhasebe')");
            await musteriFrame.HoverAsync("font:has-text('Fiş İşlemleri')");
            await musteriFrame.ClickAsync("font:has-text('Fiş Girişi')");
            await musteriFrame.WaitForTimeoutAsync(await GetSmartDelayAsync());

            // addFis.do frame’ini bul
            IFrame? fisFrame = null;
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 20000)
            {
                frames = mmp.Context.Pages.Last().Frames;
                fisFrame = frames.FirstOrDefault(f => f.Url.Contains("addFis.do"));
                if (fisFrame != null) break;
                await Task.Delay(250);
            }
            if (fisFrame == null) return null;

            // İlk açılışta "Kaydet" → tablo initialize olsun
            try
            {
                await fisFrame.ClickAsync("button#kaydetHref");
            }
            catch { /* buton devre dışı olabilir; sorun değil */ }

            // TBL son satır Hesap Kodu inputu görünür hale gelsin
            await fisFrame.Locator("table#TBL tr:last-child input[name='HESAP_KODU']")
                          .WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 60000 });

            return fisFrame;
        }

        /// <summary>
        /// Hesap planı listesi (hsptable) görünür değilse F9 ile açar.
        /// </summary>
        private static async Task EnsureHesapPlaniListVisibleAsync(IFrame fisFrame)
        {
            try
            {
                var firstRow = fisFrame.Locator("table#hsptable tbody#hstbody tr").First;
                if (await firstRow.IsVisibleAsync(new() { Timeout = 1200 }))
                    return;
            }
            catch { /* görünür değil */ }

            // F9 ile aç
            var hesapInput = fisFrame.Locator("table#TBL tr:last-child input[name='HESAP_KODU']");
            await hesapInput.ClickAsync();
            await hesapInput.PressAsync("F9");

            await fisFrame.WaitForSelectorAsync("table#hsptable tbody#hstbody tr",
                new() { State = WaitForSelectorState.Attached, Timeout = 60000 });
        }

        #endregion
    }

    /// <summary>
    /// Oturum ve cache tutucu (sade).
    /// </summary>
    public static class LucaSession
    {
        public static IPage? MmpPage { get; set; }
        public static IFrame? Frame { get; set; }
        public static List<AccountingCode>? CachedHesapPlani { get; set; }
    }
}
