using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TurkSoft.GIBWebUI.AppSettings;

namespace TurkSoft.GIBWebUI.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<LoginController> _logger;

        public LoginController(
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment env,
            IOptions<ApiSettings> apiOptions,
            ILogger<LoginController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _env = env;
            _apiSettings = apiOptions.Value;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Login sonrası, doğrudan çağrılabilen asıl iş metodu.
        /// GİB’den ZIP indirir, JSON’u wwwroot/data/gibusers_invoice_receipt_list.json olarak yazar.
        /// POST /Login/StartMukellefSync?userId=11
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> StartMukellefSync(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Geçersiz userId.");
            }

            var baseUrl = _apiSettings.GibPortalApiBaseUrl?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                _logger.LogError("GibPortalApiBaseUrl tanımlı değil. Mükellef sync yapılamadı.");
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    "GibPortalApiBaseUrl tanımlı değil.");
            }

            var client = _httpClientFactory.CreateClient();

            var url = $"{baseUrl}/TurkcellEFatura/gibuser/recipient-zip?userId={userId}";
            _logger.LogInformation("[GIB] StartMukellefSync başlıyor. userId={UserId}, Url={Url}", userId, url);

            HttpResponseMessage resp;
            try
            {
                resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GIB] ZIP isteği atılırken hata oluştu.");
                return StatusCode((int)HttpStatusCode.BadGateway,
                    "GİB ZIP isteğinde hata oluştu: " + ex.Message);
            }

            if (!resp.IsSuccessStatusCode)
            {
                var body = string.Empty;
                try { body = await resp.Content.ReadAsStringAsync(); } catch { }

                _logger.LogError(
                    "[GIB] ZIP isteği başarısız. Status={Status} {Reason}. Body(ilk200)={Body}",
                    (int)resp.StatusCode,
                    resp.ReasonPhrase,
                    body?.Length > 200 ? body.Substring(0, 200) : body
                );

                return StatusCode((int)resp.StatusCode,
                    "GİB ZIP isteği başarısız: " + (int)resp.StatusCode + " " + resp.ReasonPhrase);
            }

            await using var respStream = await resp.Content.ReadAsStreamAsync();

            using var zip = new ZipArchive(respStream, ZipArchiveMode.Read);

            // Önce tam isim
            var entry = zip.Entries
                .FirstOrDefault(e =>
                    string.Equals(e.Name, "gibusers_invoice_receipt_list.json",
                        StringComparison.OrdinalIgnoreCase));

            // Yoksa ilk JSON
            if (entry == null)
            {
                entry = zip.Entries
                    .FirstOrDefault(e => e.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
            }

            if (entry == null)
            {
                _logger.LogError("[GIB] ZIP içinde JSON dosyası bulunamadı.");
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    "ZIP içerisinde JSON dosyası bulunamadı.");
            }

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var dataDir = Path.Combine(webRoot, "data");
            Directory.CreateDirectory(dataDir);

            var outPath = Path.Combine(dataDir, "gibusers_invoice_receipt_list.json");

            _logger.LogInformation("[GIB] JSON dosyası {FileName} -> {OutPath} olarak yazılıyor.",
                entry.FullName, outPath);

            // 🔴 ÖNEMLİ: FileShare.Read ile, biz yazarken başka process'ler dosyayı okuyabilsin
            await using (var entryStream = entry.Open())
            await using (var fileStream = new FileStream(
                outPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read))
            {
                await entryStream.CopyToAsync(fileStream);
            }

            _logger.LogInformation("[GIB] Mükellef JSON yazıldı. Path={OutPath}", outPath);

            // Frontend tarafı için web yolu:
            var webPath = "/data/gibusers_invoice_receipt_list.json";

            return Ok(new
            {
                success = true,
                path = webPath
            });
        }

        /// <summary>
        /// Login sonrası arka planda mükellef listesini yeniler.
        /// GET /Login/RefreshMukellef?userId=11
        /// </summary>
        [HttpGet]
        public IActionResult RefreshMukellef(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Geçersiz userId.");
            }

            _logger.LogInformation("[GIB] RefreshMukellef tetiklendi. userId={UserId}", userId);

            // Fire & forget: İstek beklemesin, ağır işi arka planda çalıştır.
            _ = Task.Run(async () =>
            {
                try
                {
                    await StartMukellefSync(userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "[GIB] RefreshMukellef arka plan işinde hata oluştu. userId={UserId}",
                        userId);
                }
            });

            return Ok(new
            {
                success = true,
                message = "Mükellef listesi arka planda güncelleniyor."
            });
        }
    }
}
