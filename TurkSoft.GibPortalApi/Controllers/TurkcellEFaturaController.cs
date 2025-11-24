using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;              // ✅ EF Core
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TurkSoft.Data.GibData;                     // ✅ GibAppDbContext burada
using TurkSoft.Entities.GIBEntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.GibPortalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TurkcellEFaturaController : ControllerBase
    {
        private readonly IGibBusiness _gib;
        private readonly GibAppDbContext _db;    // ✅ Artık EF Core context

        public TurkcellEFaturaController(IGibBusiness gibBusiness, GibAppDbContext db)
        {
            _gib = gibBusiness;
            _db = db;
        }
        public static class TestInvoices
        {
            // 1) Basit perakende fatura – 1 kalem, KDV %18
            public static Invoice CreateInvoice1()
            {
                var customer = new Customer
                {
                    Id = 1,
                    Name = "Ali",
                    Surname = "Yılmaz",
                    Phone = "05551112233",
                    Email = "ali.yilmaz@test.com",
                    TaxNo = "1234567803",
                    TaxOffice = "Üsküdar"
                };

                var unit = new Unit
                {
                    Id = 1,
                    Name = "Adet",
                    ShortName = "C62"
                };

                var item = new Item
                {
                    Id = 1,
                    Name = "Asus Laptop",
                    Code = "LAPTOP-001",
                    BrandId = 1,
                    UnitId = unit.Id,
                    Unit = unit,
                    Price = 100m,
                    Currency = "TRY"
                };

                var invoice = new Invoice
                {
                    Id = 1,
                    CustomerId = customer.Id,
                    Customer = customer,
                    InvoiceNo = "INV-2025-0001",
                    InvoiceDate = new DateTime(2025, 1, 3),
                    Currency = "TRY",
                    Total = 118m
                };

                var invItem = new InvoicesItem
                {
                    Id = 1,
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    ItemId = item.Id,
                    Item = item,
                    Quantity = 1m,
                    Price = 100m,
                    Total = 100m
                };

                var invTax = new InvoicesTax
                {
                    Id = 1,
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    Name = "KDV",
                    Rate = 18m,
                    Amount = 18m
                };

                invoice.InvoicesItems = new List<InvoicesItem> { invItem };
                invoice.InvoicesTaxes = new List<InvoicesTax> { invTax };

                return invoice;
            }

            // 2) Hizmet faturası – 2 saat danışmanlık, KDV %10
            public static Invoice CreateInvoice2()
            {
                var customer = new Customer
                {
                    Id = 2,
                    Name = "Ahmet",
                    Surname = "Demir",
                    Phone = "05553334455",
                    Email = "ahmet.demir@test.com",
                    TaxNo = "1234567803",
                    TaxOffice = "Kadıköy"
                };

                var unit = new Unit
                {
                    Id = 2,
                    Name = "Saat",
                    ShortName = "HUR"
                };

                var item = new Item
                {
                    Id = 2,
                    Name = "Danışmanlık Hizmeti",
                    Code = "SRV-CONS-001",
                    BrandId = 1,
                    UnitId = unit.Id,
                    Unit = unit,
                    Price = 50m,
                    Currency = "TRY"
                };

                var invoice = new Invoice
                {
                    Id = 2,
                    CustomerId = customer.Id,
                    Customer = customer,
                    InvoiceNo = "INV-2025-0002",
                    InvoiceDate = new DateTime(2025, 1, 4),
                    Currency = "TRY",
                    Total = 110m
                };

                var invItem = new InvoicesItem
                {
                    Id = 2,
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    ItemId = item.Id,
                    Item = item,
                    Quantity = 2m,     // 2 saat
                    Price = 50m,
                    Total = 100m
                };

                var invTax = new InvoicesTax
                {
                    Id = 2,
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    Name = "KDV",
                    Rate = 10m,
                    Amount = 10m
                };

                invoice.InvoicesItems = new List<InvoicesItem> { invItem };
                invoice.InvoicesTaxes = new List<InvoicesTax> { invTax };

                return invoice;
            }

            // 3) 2 kalem ürün, KDV %20
            public static Invoice CreateInvoice3()
            {
                var customer = new Customer
                {
                    Id = 3,
                    Name = "Mehmet",
                    Surname = "Kaya",
                    Phone = "05557778899",
                    Email = "mehmet.kaya@test.com",
                    TaxNo = "1234567803",
                    TaxOffice = "Beşiktaş"
                };

                var unit = new Unit
                {
                    Id = 3,
                    Name = "Adet",
                    ShortName = "C62"
                };

                var item1 = new Item
                {
                    Id = 3,
                    Name = "Ofis Sandalyesi",
                    Code = "OFF-CHR-001",
                    BrandId = 1,
                    UnitId = unit.Id,
                    Unit = unit,
                    Price = 50m,
                    Currency = "TRY"
                };

                var item2 = new Item
                {
                    Id = 4,
                    Name = "Ofis Masası",
                    Code = "OFF-TBL-001",
                    BrandId = 1,
                    UnitId = unit.Id,
                    Unit = unit,
                    Price = 80m,
                    Currency = "TRY"
                };

                var invoice = new Invoice
                {
                    Id = 3,
                    CustomerId = customer.Id,
                    Customer = customer,
                    InvoiceNo = "INV-2025-0003",
                    InvoiceDate = new DateTime(2025, 1, 5),
                    Currency = "TRY",
                    Total = 216m   // 100 + 80 = 180; %20 KDV = 36; toplam 216
                };

                var invItem1 = new InvoicesItem
                {
                    Id = 3,
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    ItemId = item1.Id,
                    Item = item1,
                    Quantity = 2m,
                    Price = 50m,
                    Total = 100m
                };

                var invItem2 = new InvoicesItem
                {
                    Id = 4,
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    ItemId = item2.Id,
                    Item = item2,
                    Quantity = 1m,
                    Price = 80m,
                    Total = 80m
                };

                var invTax = new InvoicesTax
                {
                    Id = 3,
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    Name = "KDV",
                    Rate = 20m,
                    Amount = 36m
                };

                invoice.InvoicesItems = new List<InvoicesItem> { invItem1, invItem2 };
                invoice.InvoicesTaxes = new List<InvoicesTax> { invTax };

                return invoice;
            }

            // 4) İhracat faturası – USD, KDV 0
            public static Invoice CreateInvoice4()
            {
                var customer = new Customer
                {
                    Id = 4,
                    Name = "Export Ltd",
                    Surname = "",
                    Phone = "+90 212 555 55 55",
                    Email = "export@test.com",
                    TaxNo = "1234567803",
                    TaxOffice = "İstanbul"
                };

                var unit = new Unit
                {
                    Id = 4,
                    Name = "Adet",
                    ShortName = "C62"
                };

                var item = new Item
                {
                    Id = 5,
                    Name = "Export Product",
                    Code = "EXP-PRD-001",
                    BrandId = 1,
                    UnitId = unit.Id,
                    Unit = unit,
                    Price = 300m,
                    Currency = "USD"
                };

                var invoice = new Invoice
                {
                    Id = 4,
                    CustomerId = customer.Id,
                    Customer = customer,
                    InvoiceNo = "EXP-2025-0001",
                    InvoiceDate = new DateTime(2025, 1, 10),
                    Currency = "USD",
                    Total = 300m
                };

                var invItem = new InvoicesItem
                {
                    Id = 5,
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    ItemId = item.Id,
                    Item = item,
                    Quantity = 1m,
                    Price = 300m,
                    Total = 300m
                };

                var invTax = new InvoicesTax
                {
                    Id = 4,
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    Name = "KDV",
                    Rate = 0m,
                    Amount = 0m
                };

                invoice.InvoicesItems = new List<InvoicesItem> { invItem };
                invoice.InvoicesTaxes = new List<InvoicesTax> { invTax };

                return invoice;
            }
        }
        public class SendEInvoiceUblRequest
        {
            public long InvoiceId { get; set; }
            public int AppType { get; set; }
            public int Status { get; set; }
            public bool UseManualInvoiceId { get; set; }
            public string? TargetAlias { get; set; }
            public bool? UseFirstAlias { get; set; }
            public string? Prefix { get; set; }
            public string? LocalReferenceId { get; set; }
            public bool? CheckLocalReferenceId { get; set; }
            public string? XsltCode { get; set; }

            public IFormFile InvoiceFile { get; set; } = null!;
        }


        // EF Core helper – faturayı ilişkileriyle beraber yükler
        // ve sonucu TestInvoices.CreateInvoiceX yapısına benzer bir grafik olarak döner.
        private async Task<Invoice?> LoadInvoiceAsync(long id, CancellationToken ct)
        {
            // 1) DB’den faturayı ilişkileriyle al
            var dbInvoice = await _db.Set<Invoice>()
                .IgnoreQueryFilters() // IsActive filtresi devre dışı (silinmiş olsa bile gör)
                .AsNoTracking()       // Sadece okuma
                .Include(i => i.Customer)
                .Include(i => i.InvoicesItems)
                    .ThenInclude(ii => ii.Item)
                        .ThenInclude(it => it.Unit)
                .Include(i => i.InvoicesTaxes)
                .FirstOrDefaultAsync(i => i.Id == id, ct);

            if (dbInvoice == null)
            {
                // BURADA gerçekten DB'de bu Id ile kayıt yok demektir.
                // İstersen null dön, istersen aşağıdaki fallback'li versiyonu kullan.
                return null;
            }

            // 2) TestInvoices.CreateInvoiceX yapısına benzer bir grafik oluştur
            return MapToTestInvoiceShape(dbInvoice);
        }


        /// <summary>
        /// Veritabanından gelen Invoice grafiğini
        /// TestInvoices.CreateInvoiceX fonksiyonlarındaki gibi
        /// yeni bir Invoice objesine map eder.
        /// </summary>
        private static Invoice MapToTestInvoiceShape(Invoice src)
        {
            // --- Customer ---
            Customer? customer = null;
            if (src.Customer != null)
            {
                customer = new Customer
                {
                    Id = src.Customer.Id,
                    Name = src.Customer.Name,
                    Surname = src.Customer.Surname,
                    Phone = src.Customer.Phone,
                    Email = src.Customer.Email,
                    TaxNo = src.Customer.TaxNo,
                    TaxOffice = src.Customer.TaxOffice
                    // İstersen BaseEntity alanlarını da kopyalayabilirsin
                };
            }

            // --- Invoice (root) ---
            var invoice = new Invoice
            {
                Id = src.Id,
                CustomerId = src.CustomerId,
                Customer = customer,
                InvoiceNo = src.InvoiceNo,
                InvoiceDate = src.InvoiceDate,
                Currency = src.Currency,
                Total = src.Total
                // Diğer navigation koleksiyonları (Tourists, SgkRecords vs.) şimdilik boş bırakıyoruz
            };

            // --- Cache: Aynı Unit / Item tekrar tekrar oluşturma ---
            var unitCache = new Dictionary<long, Unit>();
            var itemCache = new Dictionary<long, Item>();

            // --- InvoicesItems + Item + Unit ---
            var invItems = new List<InvoicesItem>();

            if (src.InvoicesItems != null)
            {
                foreach (var li in src.InvoicesItems)
                {
                    // Unit
                    Unit? unit = null;
                    if (li.Item?.Unit != null)
                    {
                        var u = li.Item.Unit;
                        if (u.Id != 0 && unitCache.TryGetValue(u.Id, out var cachedUnit))
                        {
                            unit = cachedUnit;
                        }
                        else
                        {
                            unit = new Unit
                            {
                                Id = u.Id,
                                Name = u.Name,
                                ShortName = u.ShortName
                            };

                            if (u.Id != 0)
                                unitCache[u.Id] = unit;
                        }
                    }

                    // Item
                    Item? item = null;
                    if (li.Item != null)
                    {
                        var it = li.Item;
                        if (it.Id != 0 && itemCache.TryGetValue(it.Id, out var cachedItem))
                        {
                            item = cachedItem;
                        }
                        else
                        {
                            item = new Item
                            {
                                Id = it.Id,
                                Name = it.Name,
                                Code = it.Code,
                                BrandId = it.BrandId,
                                UnitId = it.UnitId,
                                Unit = unit, // yukarıda oluşturduğumuz unit
                                Price = it.Price,
                                Currency = it.Currency
                            };

                            if (it.Id != 0)
                                itemCache[it.Id] = item;
                        }
                    }

                    // InvoicesItem
                    var invItem = new InvoicesItem
                    {
                        Id = li.Id,
                        InvoiceId = invoice.Id,
                        Invoice = invoice,
                        ItemId = item?.Id ?? li.ItemId,
                        Item = item,
                        Quantity = li.Quantity,
                        Price = li.Price,
                        Total = li.Total
                    };

                    invItems.Add(invItem);
                }
            }

            invoice.InvoicesItems = invItems;

            // --- InvoicesTaxes ---
            var invTaxes = new List<InvoicesTax>();
            if (src.InvoicesTaxes != null)
            {
                foreach (var tx in src.InvoicesTaxes)
                {
                    var invTax = new InvoicesTax
                    {
                        Id = tx.Id,
                        InvoiceId = invoice.Id,
                        Invoice = invoice,
                        Name = tx.Name,
                        Rate = tx.Rate,
                        Amount = tx.Amount
                    };

                    invTaxes.Add(invTax);
                }
            }

            invoice.InvoicesTaxes = invTaxes;

            return invoice;
        }


        // -------- StaticList --------
        [HttpGet("static/unit")]
        public async Task<IActionResult> GetUnitList(CancellationToken ct = default)
        {
            var res = await _gib.GetStaticListUnitAsync(ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("static/taxexemption")]
        public async Task<IActionResult> GetTaxExemption(CancellationToken ct = default)
        {
            var res = await _gib.GetStaticListTaxExemptionReasonsAsync(ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("static/withholding")]
        public async Task<IActionResult> GetWithholding(CancellationToken ct = default)
        {
            var res = await _gib.GetStaticListWithHoldingAsync(ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        // -------- e-Fatura Outbox --------
        [HttpPost("einvoice/send-json/{id:long}")]
        public async Task<IActionResult> SendEInvoiceJson(long id, bool isExport = false, CancellationToken ct = default)
        {
            var inv = await LoadInvoiceAsync(id, ct);
            if (inv == null) return NotFound();
            //Invoice inv = id switch
            //{
            //    1 => TestInvoices.CreateInvoice1(),
            //    2 => TestInvoices.CreateInvoice2(),
            //    3 => TestInvoices.CreateInvoice3(),
            //    4 => TestInvoices.CreateInvoice4(),
            //    _ => TestInvoices.CreateInvoice1()
            //};

            // İhracat için isExport parametresini Invoice4 ile beraber true gönderebilirsin
            if (id == 4)
                isExport = true;

            var res = await _gib.SendEInvoiceJsonAsync(inv, isExport, true, inv.Customer?.TaxNo, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpPost("einvoice/send-ubl")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SendEInvoiceUbl(
            [FromForm] SendEInvoiceUblRequest request,
            CancellationToken ct = default)
        {
            if (request.InvoiceFile == null || request.InvoiceFile.Length == 0)
                return BadRequest("UBL dosyası gerekli.");

            var inv = await LoadInvoiceAsync(request.InvoiceId, ct);
            if (inv == null) return NotFound();

            await using var ms = new System.IO.MemoryStream();
            await request.InvoiceFile.CopyToAsync(ms, ct);
            ms.Position = 0;

            var res = await _gib.SendEInvoiceUblAsync(
                ms,
                request.InvoiceFile.FileName,
                request.AppType,
                request.Status,
                request.UseManualInvoiceId,
                request.TargetAlias,
                request.UseFirstAlias,
                request.Prefix,
                request.LocalReferenceId,
                request.CheckLocalReferenceId,
                request.XsltCode,
                true,
                inv.Customer?.TaxNo,
                ct);

            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("einvoice/outbox/pdf/{ettn:guid}")]
        public async Task<IActionResult> GetEInvoiceOutboxPdf(Guid ettn, bool standardXslt = true, CancellationToken ct = default)
        {
            var res = await _gib.GetEInvoiceOutboxPdfAsync(ettn, standardXslt, ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);
            return File(res.Data!, "application/pdf", $"{ettn}.pdf");
        }
        [HttpGet("einvoice/outbox/ubl/{ettn:guid}")]
        public async Task<IActionResult> GetEInvoiceOutboxUbl(Guid ettn, bool standardXslt = true, CancellationToken ct = default)
        {
            var res = await _gib.GetEInvoiceOutboxUblAsync(ettn, ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);
            
            // UBL genelde XML olarak döner
            return File(res.Data, "application/xml", $"{ettn}.xml");
        }
        // -------- e-Fatura Inbox --------
        [HttpGet("einvoice/inbox/list")]
        public async Task<IActionResult> GetInboxList(DateTime start, DateTime end, int pageIndex = 1, int pageSize = 100, bool isNew = true, CancellationToken ct = default)
        {
            var res = await _gib.GetEInvoiceInboxAsync(start, end, pageIndex, pageSize, isNew, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("einvoice/inbox/pdf/{ettn:guid}")]
        public async Task<IActionResult> GetEInvoiceInboxPdf(Guid ettn, bool standardXslt = true, CancellationToken ct = default)
        {
            var res = await _gib.GetEInvoiceInboxPdfAsync(ettn, standardXslt, ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);
            return File(res.Data!, "application/pdf", $"{ettn}.pdf");
        }
          [HttpGet("einvoice/inbox/ubl/{ettn:guid}")]
        public async Task<IActionResult> GetEInvoiceInboxUBL(Guid ettn, CancellationToken ct = default)
        {
            var res = await _gib.GetEInvoiceInboxUblAsync(ettn, ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);
            return File(res.Data!, "application/pdf", $"{ettn}.pdf");
        }
        [HttpPost("einvoice/response")]
        public async Task<IActionResult> SendInvoiceResponse([FromBody] InvoiceResponseRequest request, CancellationToken ct = default)
        {
            var res = await _gib.SendInvoiceResponseAsync(request, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        // -------- e-Arşiv --------
        [HttpPost("earchive/send-json/{id:long}")]
        public async Task<IActionResult> SendEArchiveJson(long id, CancellationToken ct = default)
        {
            //var inv = await LoadInvoiceAsync(id, ct);
            //if (inv == null) return NotFound();
            Invoice inv = id switch
            {
                1 => TestInvoices.CreateInvoice1(), // Basit perakende
                2 => TestInvoices.CreateInvoice2(), // Hizmet
                3 => TestInvoices.CreateInvoice3(), // Çok kalemli
                4 => TestInvoices.CreateInvoice4(), // İhracat (e-Arşiv de test edebilirsin)
                _ => TestInvoices.CreateInvoice1()
            };


            var res = await _gib.SendEArchiveJsonAsync(inv, true, inv.Customer?.TaxNo, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpPost("earchive/send-json-raw")]
        public async Task<IActionResult> SendEArchiveJsonRaw([FromBody] JsonElement body, CancellationToken ct = default)
        {
            var res = await _gib.SendEArchiveJsonRawAsync(body, true, null, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("earchive/pdf/{ettn:guid}")]
        public async Task<IActionResult> GetEArchivePdf(Guid ettn, bool standardXslt = true, CancellationToken ct = default)
        {
            var res = await _gib.GetEArchivePdfAsync(ettn, standardXslt, ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);
            return File(res.Data!, "application/pdf", $"{ettn}.pdf");
        }

        [HttpPost("earchive/retry-mail/{id:guid}")]
        public async Task<IActionResult> RetryEArchiveMail(Guid id, CancellationToken ct = default)
        {
            var res = await _gib.RetryEArchiveMailAsync(id, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpPost("earchive/retry-mail-custom")]
        public async Task<IActionResult> RetryEArchiveMailCustom([FromBody] EArchiveMailRetryRequest req, CancellationToken ct = default)
        {
            var res = await _gib.RetryEArchiveMailToAddressesAsync(req, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        // -------- GIB User --------
        [HttpGet("gibuser/recipient-zip")]
        public async Task<IActionResult> GetGibUserRecipientZip(CancellationToken ct = default)
        {
            var res = await _gib.GetGibUserRecipientZipAsync(ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);
            return File(res.Data!, "application/zip", "gibuser-recipient.zip");
        }
    }
}
