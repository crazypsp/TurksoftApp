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
    /// Luca otomasyon işlemlerini yöneten sınıftır.
    /// Playwright ile giriş, CAPTCHA çözme, fiş gönderimi ve hesap planı çekme işlemlerini yapar.
    /// </summary>
    public class LucaAutomationManager : ILucaAutomationBussiness
    {
        private const int CaptchaPollDelay = 5000;
        private IBrowser _browser;
        private IBrowserContext _context;
        private IPage _popup;

        /// <summary>
        /// Luca sistemine giriş işlemi gerçekleştirir. CAPTCHA çözümü yapılır.
        /// Başarılı giriş sonrası açılan MMP sayfası RAM’de saklanır.
        /// </summary>
        public async Task<IResult> LoginAsync(LucaLoginRequest user)
        {
            try
            {
                var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
                _browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
                _context = await _browser.NewContextAsync();

                var mainPage = await _context.NewPageAsync();
                await mainPage.GotoAsync("https://luca.com.tr/");

                // "Sistem" butonuna tıklanarak giriş popup'ı açılır
                await mainPage.ClickAsync("a.dropdown-toggle:has-text('Sistem')");
                _popup = await mainPage.RunAndWaitForPopupAsync(() => mainPage.ClickAsync("a[onclick*=popup]"));

                // Kullanıcı bilgileri form alanlarına yazılır
                await _popup.FillAsync("#musteriNo", user.CustumerNo);
                await _popup.FillAsync("#kullaniciAdi", user.UserName);
                await _popup.FillAsync("#parola", user.Password);
                await _popup.ClickAsync("input[value='GİRİŞ']");

                await _popup.WaitForTimeoutAsync(await GetSmartDelayAsync());

                // CAPTCHA kontrol edilir ve çözülürse çözüm akışı başlatılır
                var captchaElement = _popup.Locator("div.captcha img");
                if (await captchaElement.IsVisibleAsync())
                    await HandleCaptchaAsync(user, captchaElement);

                // Giriş sonrası yönlendirme yapılır
                await _popup.ClickAsync(".lucaMmpLogo");
                await _popup.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // SSO yönlendirme kontrolü (örnek: ssoGiris.do)
                var currentUrl = _popup.Url;
                // Eğer yönlendirme ssoGiris.do ise yeni popup beklenir (örneğin login sonrası otomatik başka pencere açılır)
                if (currentUrl.Contains("ssoGiris.do"))
                {
                    // Giriş sonrası otomatik yeni popup penceresi geliyor olabilir
                    var redirectedPopup = await _popup.WaitForPopupAsync(new PageWaitForPopupOptions
                    {
                        Timeout = 10000
                    });

                    await redirectedPopup.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    LucaSession.MmpPage = redirectedPopup;
                }
                else
                {
                    // Doğrudan popup üzerinden işlem yapılacaksa mevcut popup tutulur
                    LucaSession.MmpPage = _popup;
                }

                return new SuccessResult("Luca sistemine başarıyla giriş yapıldı.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Giriş hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Luca içerisinden hesap planı verileri çekilir.
        /// Veri RAM’e alınır ve sonraki işlemlerde tekrar kullanım sağlanır.
        /// </summary>
        public async Task<IDataResult<List<AccountingCode>>> GetAccountingPlanAsync()
        {
            try
            {
                // 🔐 Aktif oturumdan MMP sayfasını al
                var mmp = LucaSession.MmpPage;

                // 🔁 Açık tüm çerçeveleri al
                var frames = mmp.Context.Pages.Last().Frames;

                // 📌 Menü ve müşteri bilgileri frame'lerini bul
                var menuFrame = frames.FirstOrDefault(f => f.Url.Contains("menu.do") || f.Name == "frm2");
                var musteriFrame = frames.FirstOrDefault(f => f.Url.Contains("musteriBilgileri.do") || f.Name == "frm3");

                var list = new List<AccountingCode>();

                // ✅ Menü varsa ilerle
                if (menuFrame != null && musteriFrame != null)
                {
                    // 👆 Üst menüde "Muhasebe" üzerine gel
                    await menuFrame.HoverAsync("font:has-text('Muhasebe')");

                    // 👇 Alt menüde "Fiş İşlemleri" üzerine gel ve tıkla
                    await musteriFrame.HoverAsync("font:has-text('Fiş İşlemleri')");
                    await musteriFrame.ClickAsync("font:has-text('Fiş Girişi')");

                    // ⏱️ Fiş ekranının yüklenmesini bekle
                    await musteriFrame.WaitForTimeoutAsync(await GetSmartDelayAsync());

                    // 📄 Fiş ekranını temsil eden frame'i bul
                    var fisFrame = frames.FirstOrDefault(f => f.Url.Contains("addFis.do"));
                    if (fisFrame != null)
                    {
                        // 📤 İlk sayfa açıldığında boş fişi kaydet tuşuna basılır
                        await fisFrame.ClickAsync("button#kaydetHref");

                        // 🔄 Sayfa yüklenmesini bekle
                        await fisFrame.WaitForLoadStateAsync(LoadState.NetworkIdle);

                        // 🔍 Hesap planı satırlarını bul
                        await fisFrame.WaitForSelectorAsync("table#hsptable tbody#hstbody tr");
                        var rows = fisFrame.Locator("table#hsptable tbody#hstbody tr");
                        int count = await rows.CountAsync();

                        for (int i = 0; i < count; i++)
                        {
                            var row = rows.Nth(i);
                            var code = (await row.Locator("td:nth-child(1)").InnerTextAsync())?.Trim();
                            var name = (await row.Locator("td:nth-child(2)").InnerTextAsync())?.Trim();

                            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(name))
                                list.Add(new AccountingCode { Code = code, Name = name });
                        }
                        LucaSession.Frame = fisFrame;
                    }
                }

                // 🧠 RAM'e al
                LucaSession.CachedHesapPlani = list;

                return new DataResult<List<AccountingCode>>(list, true, "Hesap planı başarıyla alındı.");
            }
            catch (Exception ex)
            {
                return new DataResult<List<AccountingCode>>(null, false, $"Plan çekme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Luca'ya fiş satırlarını gönderir. 600 satırdan sonra yeni fiş oluşturur.
        /// </summary>
        public async Task<IResult> SendFisRowsAsync(List<LucaFisRow> rows)
        {
            try
            {
                var fisFrame = LucaSession.Frame;
                const string tableSelector = "table#TBL";
                bool newFisStart = true;

                const int BATCH_SIZE = 400;
                int rowsInCurrentFis = 0;

                for (int i = 0; i < rows.Count; i++)
                {
                    // --- SATIR EKLEME ---
                    if (!newFisStart)
                    {
                        await fisFrame.ClickAsync($"{tableSelector} tr:last-child td.add_delete.btn-td input[value='+']");
                        await fisFrame.WaitForSelectorAsync($"{tableSelector} tr:last-child input[name='HESAP_KODU']");
                    }
                    newFisStart = false;

                    var currentRowSelector = $"{tableSelector} tr:last-child";
                    var r = rows[i];

                    // -----------------------------
                    // HESAP KODU – SAĞLAM DOLDURMA
                    // -----------------------------
                    if (string.IsNullOrWhiteSpace(r.HesapKodu))
                        throw new InvalidOperationException($"[{i + 1}. satır] Hesap Kodu boş olamaz.");

                    var hesapInput = fisFrame.Locator($"{currentRowSelector} input[name='HESAP_KODU']");
                    await hesapInput.ClickAsync();                  // odayı bu hücreye getir
                    await hesapInput.FillAsync(r.HesapKodu.Trim()); // doğrudan yaz
                    await hesapInput.PressAsync("Tab");             // blur/validate

                    // küçük bir bekleme ve doğrulama
                    await fisFrame.WaitForTimeoutAsync(120);
                    var typed = (await hesapInput.InputValueAsync())?.Trim();

                    if (string.IsNullOrEmpty(typed))
                    {
                        // Yedek yol: F9 ile hesap seçimi aç, metne göre bul ve çift tıkla
                        await hesapInput.ClickAsync();
                        await hesapInput.PressAsync("F9");

                        // liste açılana kadar bekle
                        await fisFrame.WaitForSelectorAsync("table#hsptable");
                        var rowInList = fisFrame.Locator("table#hsptable tr").Filter(new() { HasText = r.HesapKodu.Trim() });
                        await rowInList.First.DblClickAsync();

                        // tekrar doğrula
                        await fisFrame.WaitForTimeoutAsync(120);
                        typed = (await hesapInput.InputValueAsync())?.Trim();
                        if (string.IsNullOrEmpty(typed))
                            throw new Exception($"[{i + 1}. satır] Hesap Kodu '{r.HesapKodu}' alanına yazılamadı.");
                    }

                    // -----------------------------
                    // Diğer alanlar
                    // -----------------------------
                    await fisFrame.FillAsync($"{currentRowSelector} input[name='EVRAK_NO']", r.EvrakNo);
                    await fisFrame.FillAsync($"{currentRowSelector} input[name='EVRAK_TARIHI']",
                        r.Tarih.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("tr-TR")));
                    await fisFrame.FillAsync($"{currentRowSelector} input[name='BELGE_TUR_KOD']", "MK");
                    await fisFrame.FillAsync($"{currentRowSelector} input[name='ACIKLAMA']", r.Aciklama);
                    await fisFrame.FillAsync($"{currentRowSelector} input[name='BORC']", ToMoney(r.Borc));
                    await fisFrame.FillAsync($"{currentRowSelector} input[name='ALACAK']", ToMoney(r.Alacak));
                    await fisFrame.FillAsync($"{currentRowSelector} input[name='MIKTAR']", "0,00000");

                    rowsInCurrentFis++;

                    // --- 400'de bir kaydet + yeni fiş ---
                    if (rowsInCurrentFis >= BATCH_SIZE)
                    {
                        await Task.WhenAll(
                            fisFrame.ClickAsync("button#kaydetHref"),
                            fisFrame.WaitForLoadStateAsync(LoadState.NetworkIdle)
                        );

                        await fisFrame.ClickAsync("button#yeniHref");

                        var sw = Stopwatch.StartNew();
                        IFrame? newFrame = null;
                        while (sw.ElapsedMilliseconds < 15000)
                        {
                            var frames = LucaSession.MmpPage.Context.Pages.Last().Frames;
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
                        newFisStart = true;
                        rowsInCurrentFis = 0;

                        await fisFrame.ClickAsync("button#kaydetHref");
                        await fisFrame.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    }
                }



                // Fişi kaydet ve bağlantıyı kapat
                await fisFrame.ClickAsync("button#kaydetHref");
                await LucaSession.MmpPage.Context.CloseAsync();
                //await _browser.CloseAsync();

                return new SuccessResult("Fiş satırları başarıyla gönderildi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Fiş gönderme hatası: {ex.Message}");
            }
        }


        /// <summary>
        /// CAPTCHA çözme işlemi - 2Captcha ile entegre.
        /// </summary>
        private async Task HandleCaptchaAsync(LucaLoginRequest user, ILocator captchaElement)
        {
            bool solved = false;
            while (!solved)
            {
                var path = Path.Combine(Path.GetTempPath(), "captcha.png");
                await captchaElement.ScreenshotAsync(new LocatorScreenshotOptions { Path = path });
                var text = await SolveCaptchaAsync(path, user.ApiKey);

                await _popup.FillAsync("#captcha-input", text);
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

        /// <summary>
        /// CAPTCHA çözümünü 2Captcha üzerinden yapar.
        /// </summary>
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
            var obj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            string id = obj["request"].GetString();

            while (true)
            {
                await Task.Delay(CaptchaPollDelay);
                var check = await client.GetStringAsync($"http://2captcha.com/res.php?key={apiKey}&action=get&id={id}&json=1");
                var res = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(check);
                if (res["status"].GetInt32() == 1)
                    return res["request"].GetString();
            }
        }

        /// <summary>
        /// Hesap kodlarında nokta ve rakam kaçış karakteri ekler.
        /// </summary>
        private static string EscapeCssId(string code)
        {
            if (char.IsDigit(code[0]))
                return $"\\3{code[0]} {code.Substring(1).Replace(".", "\\.")}";
            return code.Replace(".", "\\.");
        }

        /// <summary>
        /// Parayı string olarak formatlar (TR kültürüne uygun).
        /// </summary>
        private static string ToMoney(decimal v)
            => v.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"));

        /// <summary>
        /// Kullanıcının internet hızına göre bekleme süresi belirler.
        /// </summary>
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

        public Task<IDataResult<List<CompanyCode>>> GetCompanyAsync()
        {
            throw new NotImplementedException();
        }
    }
}
