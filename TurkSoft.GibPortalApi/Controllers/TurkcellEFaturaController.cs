using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;              // ✅ EF Core
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly GibAppDbContext _db;    // ✅ EF Core context

        public TurkcellEFaturaController(IGibBusiness gibBusiness, GibAppDbContext db)
        {
            _gib = gibBusiness;
            _db = db;
        }

        /// <summary>
        /// Her istekten önce UserId'ye göre firma bulup
        /// iş katmanındaki Options(ApiKey, VKN, Alias) değerlerini günceller.
        /// Firma bulunamazsa 404 döndürmek için IActionResult üretir.
        /// </summary>
        private async Task<IActionResult?> ApplyGibFirmForUserAsync(long userId, CancellationToken ct)
        {
            // db.Gibfirm tablosundan UserId ile eşleşen firmayı çek
            // Buradaki GibFirm ve property adını kendi entity / DbSet adına göre düzelt.
            var firm = await _db.GibFirm
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId, ct);

            if (firm == null)
            {
                return NotFound($"UserId {userId} için GIB firma kaydı bulunamadı.");
            }

            // ⚠️ BURAYI KENDİ ENTITY'NE GÖRE DÜZENLEMEN GEREKİYOR
            // Alias property’si sende farklı isimde olabilir (ör: InboxAlias, GibAlias, AliasName vs.)
            // Örneğin: var alias = firm.InboxAlias;
            var alias = firm.GibAlias; // <--- EĞER entity'nde InboxAlias yoksa, BURAYI kendi alan adına göre değiştir

            // İş katmanındaki Options değerlerini güncelle
            _gib.UpdateUserOptions(firm.ApiKey, firm.TaxNo, alias);

            return null;
        }

        // ✅ EK: kontör vb. işlemlerde firma VKN'ini lazımsa çekmek için helper
        private async Task<string?> GetFirmTaxNoAsync(long userId, CancellationToken ct)
        {
            var firm = await _db.GibFirm.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, ct);
            return firm?.TaxNo;
        }

        // -------- TestInvoices --------
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

        // =========================
        // ✅ EK: Yeni request DTO'ları
        // =========================
        public class UpdateStatusListRequest
        {
            public List<Guid> Ids { get; set; } = new();
            public int Status { get; set; }
        }

        public class UpdateEInvoiceUblRequest
        {
            public Guid Ettn { get; set; }
            public int AppType { get; set; }
            public int Status { get; set; }
            public bool UseManualInvoiceId { get; set; }
            public string? TargetAlias { get; set; }
            public bool? UseFirstAlias { get; set; }
            public string? LocalReferenceId { get; set; }
            public bool? CheckLocalReferenceId { get; set; }
            public string? XsltCode { get; set; }
            public IFormFile InvoiceFile { get; set; } = null!;
        }

        public class ConsumeBalanceRequest
        {
            public string? CompanyVkn { get; set; }
            public int Amount { get; set; } = 1;
            public string Reason { get; set; } = "Manual consume";
        }

        public class SendEArchiveUblRequest
        {
            public int Status { get; set; }
            public bool UseManualInvoiceId { get; set; }
            public bool SendEmail { get; set; } = true;
            public string? EmailAddress { get; set; }
            public string? Prefix { get; set; }
            public string? LocalReferenceId { get; set; }
            public bool? CheckLocalReferenceId { get; set; }
            public string? XsltCode { get; set; }
            public bool? AllowOldEArsivCustomer { get; set; }
            public IFormFile InvoiceFile { get; set; } = null!;
        }

        public class UpdateEArchiveUblRequest
        {
            public Guid Id { get; set; }
            public int Status { get; set; }
            public bool SendEmail { get; set; } = true;
            public string EmailAddress { get; set; } = default!;
            public string? LocalReferenceId { get; set; }
            public bool? CheckLocalReferenceId { get; set; }
            public string? XsltCode { get; set; }
            public bool? AllowOldEArsivCustomer { get; set; }
            public IFormFile InvoiceFile { get; set; } = null!;
        }

        public class PostEDefterFormRequest
        {
            public string StartDate { get; set; } = default!;
            public string EndDate { get; set; } = default!;
            public int SplitSize { get; set; } = 50;
            public bool TimeStamp { get; set; }
            public bool WithoutTaxDetail { get; set; }
            public IFormFile ZipFile { get; set; } = null!;
        }

        // EF Core helper – faturayı ilişkileriyle beraber yükler
        private async Task<Invoice?> LoadInvoiceAsync(long id, CancellationToken ct)
        {
            var dbInvoice = await _db.Set<Invoice>()
                .IgnoreQueryFilters()
                .AsNoTracking()

                // -------- CUSTOMER + ADDRESS + GROUP --------
                .Include(i => i.Customer)
                    .ThenInclude(c => c.Addresses)
                .Include(i => i.Customer)
                    .ThenInclude(c => c.CustomersGroups)
                        .ThenInclude(cg => cg.Group)

                // -------- ITEM TARAFI (InvoiceLines) --------
                .Include(i => i.InvoicesItems)
                    .ThenInclude(ii => ii.Item)
                        .ThenInclude(it => it.Unit)
                .Include(i => i.InvoicesItems)
                    .ThenInclude(ii => ii.Item)
                        .ThenInclude(it => it.Brand)
                .Include(i => i.InvoicesItems)
                    .ThenInclude(ii => ii.Item)
                        .ThenInclude(it => it.ItemsCategories)
                            .ThenInclude(ic => ic.Category)
                .Include(i => i.InvoicesItems)
                    .ThenInclude(ii => ii.Item)
                        .ThenInclude(it => it.ItemsDiscounts)
                .Include(i => i.InvoicesItems)
                    .ThenInclude(ii => ii.Item)
                        .ThenInclude(it => it.Identifiers)

                // -------- FATURA DÜZEYİ VERGİ / İSKONTO --------
                .Include(i => i.InvoicesTaxes)
                .Include(i => i.InvoicesDiscounts)
                    .ThenInclude(d => d.Item)
                        .ThenInclude(it => it.Unit)
                .Include(i => i.InvoicesDiscounts)
                    .ThenInclude(d => d.Item)
                        .ThenInclude(it => it.Brand)

                // -------- TURİST / SGK / HİZMET SAĞLAYICI / İADE --------
                .Include(i => i.Tourists)
                .Include(i => i.SgkRecords)
                .Include(i => i.ServicesProviders)
                .Include(i => i.Returns)

                // -------- ÖDEME --------
                .Include(i => i.InvoicesPayments)
                    .ThenInclude(ip => ip.Payment)
                        .ThenInclude(p => p.PaymentType)
                .Include(i => i.InvoicesPayments)
                    .ThenInclude(ip => ip.Payment)
                        .ThenInclude(p => p.PaymentAccount)
                            .ThenInclude(pa => pa.Bank)

                // -------- LOG + KREDİ HESABI --------
                .Include(i => i.GibInvoiceOperationLogs)
                .Include(i => i.GibUserCreditTransactions)
                    .ThenInclude(t => t.GibUserCreditAccount)
                        .ThenInclude(a => a.GibFirm)

                .FirstOrDefaultAsync(i => i.Id == id, ct);

            if (dbInvoice == null)
                return null;

            // EF tracking’den bağımsız, sade ama dolu bir graph’a çevir
            return MapToTestInvoiceShape(dbInvoice);
        }

        private static Invoice MapToTestInvoiceShape(Invoice src)
        {
            // ---- Cache'ler (aynı entity tekrar tekrar oluşturulmasın diye) ----
            var unitCache = new Dictionary<long, Unit>();
            var brandCache = new Dictionary<long, Brand>();
            var itemCache = new Dictionary<long, Item>();
            var categoryCache = new Dictionary<long, Category>();
            var payTypeCache = new Dictionary<long, PaymentType>();
            var bankCache = new Dictionary<long, Bank>();
            var payAccountCache = new Dictionary<long, PaymentAccount>();
            var paymentCache = new Dictionary<long, Payment>();
            var gibFirmCache = new Dictionary<long, GibFirm>();
            var creditAccCache = new Dictionary<long, GibUserCreditAccount>();

            // ==========================
            //  LOCAL HELPER FONKSİYONLAR
            // ==========================

            Brand? GetBrand(Brand? b)
            {
                if (b == null) return null;
                if (b.Id != 0 && brandCache.TryGetValue(b.Id, out var cached)) return cached;

                var clone = new Brand
                {
                    Id = b.Id,
                    Name = b.Name,
                    Country = b.Country,
                    Items = null
                };

                if (b.Id != 0)
                    brandCache[b.Id] = clone;

                return clone;
            }

            Unit? GetUnit(Unit? u)
            {
                if (u == null) return null;
                if (u.Id != 0 && unitCache.TryGetValue(u.Id, out var cached)) return cached;

                var clone = new Unit
                {
                    Id = u.Id,
                    Name = u.Name,
                    ShortName = u.ShortName,
                    Items = null
                };

                if (u.Id != 0)
                    unitCache[u.Id] = clone;

                return clone;
            }

            Category? GetCategory(Category? c)
            {
                if (c == null) return null;
                if (c.Id != 0 && categoryCache.TryGetValue(c.Id, out var cached)) return cached;

                var clone = new Category
                {
                    Id = c.Id,
                    Name = c.Name,
                    Desc = c.Desc,
                    ItemsCategories = null
                };

                if (c.Id != 0)
                    categoryCache[c.Id] = clone;

                return clone;
            }

            Item GetItem(Item s)
            {
                if (s.Id != 0 && itemCache.TryGetValue(s.Id, out var cached)) return cached;

                var brand = GetBrand(s.Brand);
                var unit = GetUnit(s.Unit);

                var clone = new Item
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    BrandId = brand?.Id ?? s.BrandId,
                    UnitId = unit?.Id ?? s.UnitId,
                    Price = s.Price,
                    Currency = s.Currency,
                    Brand = brand,
                    Unit = unit,
                    ItemsCategories = new List<ItemsCategory>(),
                    ItemsDiscounts = new List<ItemsDiscount>(),
                    Identifiers = new List<Identifiers>()
                };

                // ItemCategories
                if (s.ItemsCategories != null)
                {
                    foreach (var ic in s.ItemsCategories)
                    {
                        var cat = GetCategory(ic.Category);

                        clone.ItemsCategories.Add(new ItemsCategory
                        {
                            Id = ic.Id,
                            ItemId = clone.Id != 0 ? clone.Id : ic.ItemId,
                            CategoryId = cat?.Id ?? ic.CategoryId,
                            Item = clone,
                            Category = cat
                        });
                    }
                }

                // Item bazlı iskonto kayıtları
                if (s.ItemsDiscounts != null)
                {
                    foreach (var d in s.ItemsDiscounts)
                    {
                        clone.ItemsDiscounts.Add(new ItemsDiscount
                        {
                            Id = d.Id,
                            InvoiceId = d.InvoiceId,
                            Name = d.Name,
                            Rate = d.Rate,
                            Amount = d.Amount,
                            Invoice = null
                        });
                    }
                }

                // Barkod / seri vb. tanımlayıcılar
                if (s.Identifiers != null)
                {
                    foreach (var id in s.Identifiers)
                    {
                        clone.Identifiers.Add(new Identifiers
                        {
                            Id = id.Id,
                            Uuid = id.Uuid,
                            Desc = id.Desc,
                            Value = id.Value,
                            Type = id.Type,
                            ItemId = clone.Id != 0 ? clone.Id : id.ItemId,
                            Item = clone
                        });
                    }
                }

                if (s.Id != 0)
                    itemCache[s.Id] = clone;

                return clone;
            }

            PaymentType? GetPaymentType(PaymentType? s)
            {
                if (s == null) return null;
                if (s.Id != 0 && payTypeCache.TryGetValue(s.Id, out var cached)) return cached;

                var clone = new PaymentType
                {
                    Id = s.Id,
                    Name = s.Name,
                    Desc = s.Desc,
                    Payments = null
                };

                if (s.Id != 0)
                    payTypeCache[s.Id] = clone;

                return clone;
            }

            Bank? GetBank(Bank? s)
            {
                if (s == null) return null;
                if (s.Id != 0 && bankCache.TryGetValue(s.Id, out var cached)) return cached;

                var clone = new Bank
                {
                    Id = s.Id,
                    Name = s.Name,
                    SwiftCode = s.SwiftCode,
                    Country = s.Country,
                    City = s.City,
                    PaymentAccounts = null
                };

                if (s.Id != 0)
                    bankCache[s.Id] = clone;

                return clone;
            }

            PaymentAccount? GetPaymentAccount(PaymentAccount? s)
            {
                if (s == null) return null;
                if (s.Id != 0 && payAccountCache.TryGetValue(s.Id, out var cached)) return cached;

                var bank = GetBank(s.Bank);

                var clone = new PaymentAccount
                {
                    Id = s.Id,
                    Name = s.Name,
                    Desc = s.Desc,
                    BankId = bank?.Id ?? s.BankId,
                    AccountNo = s.AccountNo,
                    Iban = s.Iban,
                    Currency = s.Currency,
                    Bank = bank,
                    Payments = null
                };

                if (s.Id != 0)
                    payAccountCache[s.Id] = clone;

                return clone;
            }

            Payment? GetPayment(Payment? s)
            {
                if (s == null) return null;
                if (s.Id != 0 && paymentCache.TryGetValue(s.Id, out var cached)) return cached;

                var pt = GetPaymentType(s.PaymentType);
                var pa = GetPaymentAccount(s.PaymentAccount);

                var clone = new Payment
                {
                    Id = s.Id,
                    PaymentTypeId = pt?.Id ?? s.PaymentTypeId,
                    PaymentAccountId = pa?.Id ?? s.PaymentAccountId,
                    Amount = s.Amount,
                    Currency = s.Currency,
                    Date = s.Date,
                    Note = s.Note,
                    PaymentType = pt,
                    PaymentAccount = pa,
                    InvoicesPayments = new List<InvoicesPayment>()
                };

                if (s.Id != 0)
                    paymentCache[s.Id] = clone;

                return clone;
            }

            GibFirm? GetGibFirm(GibFirm? s)
            {
                if (s == null) return null;
                if (s.Id != 0 && gibFirmCache.TryGetValue(s.Id, out var cached)) return cached;

                var clone = new GibFirm
                {
                    Id = s.Id,
                    Title = s.Title,
                    TaxNo = s.TaxNo,
                    TaxOffice = s.TaxOffice,
                    CommercialRegistrationNo = s.CommercialRegistrationNo,
                    MersisNo = s.MersisNo,
                    AddressLine = s.AddressLine,
                    City = s.City,
                    District = s.District,
                    Country = s.Country,
                    PostalCode = s.PostalCode,
                    Phone = s.Phone,
                    Email = s.Email,
                    GibAlias = s.GibAlias,
                    ApiKey = s.ApiKey,
                    IsEInvoiceRegistered = s.IsEInvoiceRegistered,
                    IsEArchiveRegistered = s.IsEArchiveRegistered,
                    Invoices = null,
                    CreditAccounts = null
                };

                if (s.Id != 0)
                    gibFirmCache[s.Id] = clone;

                return clone;
            }

            GibUserCreditAccount? GetCreditAccount(GibUserCreditAccount? s)
            {
                if (s == null) return null;
                if (s.Id != 0 && creditAccCache.TryGetValue(s.Id, out var cached)) return cached;

                var firm = GetGibFirm(s.GibFirm);

                var clone = new GibUserCreditAccount
                {
                    Id = s.Id,
                    GibFirmId = firm?.Id ?? s.GibFirmId,
                    TotalCredits = s.TotalCredits,
                    UsedCredits = s.UsedCredits,
                    GibFirm = firm,
                    Transactions = null
                };

                if (s.Id != 0)
                    creditAccCache[s.Id] = clone;

                return clone;
            }

            // ==========================
            //  CUSTOMER + ADRESLER
            // ==========================

            Customer? customer = null;

            if (src.Customer != null)
            {
                var c = src.Customer;

                customer = new Customer
                {
                    Id = c.Id,
                    Name = c.Name,
                    Surname = c.Surname,
                    Phone = c.Phone,
                    Email = c.Email,
                    TaxNo = c.TaxNo,
                    TaxOffice = c.TaxOffice,
                    CustomersGroups = new List<CustomersGroup>(),
                    Addresses = new List<Address>(),
                    Invoices = null
                };

                // Adres listesi
                if (c.Addresses != null)
                {
                    foreach (var a in c.Addresses)
                    {
                        customer.Addresses.Add(new Address
                        {
                            Id = a.Id,
                            CustomerId = customer.Id != 0 ? customer.Id : a.CustomerId,
                            Country = a.Country,
                            City = a.City,
                            District = a.District,
                            Street = a.Street,
                            PostCode = a.PostCode,
                            Customer = customer
                        });
                    }
                }

                // Müşteri grupları
                if (c.CustomersGroups != null)
                {
                    foreach (var cg in c.CustomersGroups)
                    {
                        Group? grp = null;
                        if (cg.Group != null)
                        {
                            grp = new Group
                            {
                                Id = cg.Group.Id,
                                Name = cg.Group.Name,
                                Desc = cg.Group.Desc,
                                CustomersGroups = null
                            };
                        }

                        customer.CustomersGroups.Add(new CustomersGroup
                        {
                            Id = cg.Id,
                            CustomerId = customer.Id != 0 ? customer.Id : cg.CustomerId,
                            GroupId = grp?.Id ?? cg.GroupId,
                            Customer = customer,
                            Group = grp
                        });
                    }
                }
            }

            // ==========================
            //  ROOT INVOICE
            // ==========================

            var invoice = new Invoice
            {
                Id = src.Id,
                CustomerId = src.CustomerId,
                Customer = customer,
                InvoiceNo = src.InvoiceNo,
                InvoiceDate = src.InvoiceDate,
                Total = src.Total,
                Currency = src.Currency,

                InvoicesItems = new List<InvoicesItem>(),
                InvoicesTaxes = new List<InvoicesTax>(),
                InvoicesDiscounts = new List<InvoicesDiscount>(),
                Tourists = new List<Tourist>(),
                SgkRecords = new List<Sgk>(),
                ServicesProviders = new List<ServicesProvider>(),
                Returns = new List<Returns>(),
                InvoicesPayments = new List<InvoicesPayment>(),
                GibInvoiceOperationLogs = new List<GibInvoiceOperationLog>(),
                GibUserCreditTransactions = new List<GibUserCreditTransaction>()
            };

            // ==========================
            //  KALEMLER (InvoicesItem + Item)
            // ==========================

            if (src.InvoicesItems != null)
            {
                foreach (var li in src.InvoicesItems)
                {
                    Item? itemClone = null;
                    if (li.Item != null)
                        itemClone = GetItem(li.Item);

                    var invItem = new InvoicesItem
                    {
                        Id = li.Id,
                        InvoiceId = invoice.Id,
                        Invoice = invoice,
                        ItemId = itemClone?.Id ?? li.ItemId,
                        Item = itemClone,
                        Quantity = li.Quantity,
                        Price = li.Price,
                        Total = li.Total
                    };

                    invoice.InvoicesItems.Add(invItem);
                }
            }

            // ==========================
            //  VERGİLER
            // ==========================

            if (src.InvoicesTaxes != null)
            {
                foreach (var tx in src.InvoicesTaxes)
                {
                    invoice.InvoicesTaxes.Add(new InvoicesTax
                    {
                        Id = tx.Id,
                        InvoiceId = invoice.Id,
                        Invoice = invoice,
                        Name = tx.Name,
                        Rate = tx.Rate,
                        Amount = tx.Amount
                    });
                }
            }

            // ==========================
            //  İSKONTOLAR (InvoicesDiscount)
            // ==========================

            if (src.InvoicesDiscounts != null)
            {
                foreach (var d in src.InvoicesDiscounts)
                {
                    Item? itemClone = null;
                    if (d.Item != null)
                        itemClone = GetItem(d.Item);
                    else if (d.ItemId != 0 && itemCache.TryGetValue(d.ItemId, out var cachedItem))
                        itemClone = cachedItem;

                    invoice.InvoicesDiscounts.Add(new InvoicesDiscount
                    {
                        Id = d.Id,
                        ItemId = itemClone?.Id ?? d.ItemId,
                        Name = d.Name,
                        Desc = d.Desc,
                        Base = d.Base,
                        Rate = d.Rate,
                        Amount = d.Amount,
                        Item = itemClone
                    });
                }
            }

            // ==========================
            //  TURİST BİLGİLERİ
            // ==========================

            if (src.Tourists != null)
            {
                foreach (var t in src.Tourists)
                {
                    invoice.Tourists.Add(new Tourist
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Surname = t.Surname,
                        PassportNo = t.PassportNo,
                        PassportDate = t.PassportDate,
                        Country = t.Country,
                        City = t.City,
                        District = t.District,
                        AccountNo = t.AccountNo,
                        Bank = t.Bank,
                        Currency = t.Currency,
                        Note = t.Note,
                        InvoiceId = invoice.Id,
                        Invoice = invoice
                    });
                }
            }

            // ==========================
            //  SGK
            // ==========================

            if (src.SgkRecords != null)
            {
                foreach (var s in src.SgkRecords)
                {
                    invoice.SgkRecords.Add(new Sgk
                    {
                        Id = s.Id,
                        InvoiceId = invoice.Id,
                        Invoice = invoice,
                        Type = s.Type,
                        Code = s.Code,
                        Name = s.Name,
                        No = s.No,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate
                    });
                }
            }

            // ==========================
            //  HİZMET SAĞLAYICI
            // ==========================

            if (src.ServicesProviders != null)
            {
                foreach (var sp in src.ServicesProviders)
                {
                    invoice.ServicesProviders.Add(new ServicesProvider
                    {
                        Id = sp.Id,
                        No = sp.No,
                        SystemUser = sp.SystemUser,
                        InvoiceId = invoice.Id,
                        Invoice = invoice
                    });
                }
            }

            // ==========================
            //  İADE / İADE FATURA BİLGİSİ
            // ==========================

            if (src.Returns != null)
            {
                foreach (var r in src.Returns)
                {
                    invoice.Returns.Add(new Returns
                    {
                        Id = r.Id,
                        Number = r.Number,
                        Date = r.Date,
                        InvoiceId = invoice.Id,
                        Invoice = invoice
                    });
                }
            }

            // ==========================
            //  ÖDEMELER
            // ==========================

            if (src.InvoicesPayments != null)
            {
                foreach (var ip in src.InvoicesPayments)
                {
                    var paymentClone = GetPayment(ip.Payment);

                    invoice.InvoicesPayments.Add(new InvoicesPayment
                    {
                        Id = ip.Id,
                        InvoiceId = invoice.Id,
                        Invoice = invoice,
                        PaymentId = paymentClone?.Id ?? ip.PaymentId,
                        Payment = paymentClone
                    });

                    if (paymentClone != null)
                        paymentClone.InvoicesPayments.Add(ip); // ters navigation lazım değilse kaldırabilirsin
                }
            }

            // ==========================
            //  GİB LOG
            // ==========================

            if (src.GibInvoiceOperationLogs != null)
            {
                foreach (var log in src.GibInvoiceOperationLogs)
                {
                    invoice.GibInvoiceOperationLogs.Add(new GibInvoiceOperationLog
                    {
                        Id = log.Id,
                        InvoiceId = invoice.Id,
                        Invoice = invoice,
                        ExternalInvoiceId = log.ExternalInvoiceId,
                        InvoiceNumber = log.InvoiceNumber,
                        OperationName = log.OperationName,
                        IsSuccess = log.IsSuccess,
                        ErrorCode = log.ErrorCode,
                        ErrorMessage = log.ErrorMessage,
                        RawResponseJson = log.RawResponseJson,
                        UserId = log.UserId,
                        User = null, // User entity'sini burada taşımaya gerek yok
                        CreatedAt = log.CreatedAt
                    });
                }
            }

            // ==========================
            //  KONTÖR HAREKETLERİ
            // ==========================

            if (src.GibUserCreditTransactions != null)
            {
                foreach (var tr in src.GibUserCreditTransactions)
                {
                    var acc = GetCreditAccount(tr.GibUserCreditAccount);

                    invoice.GibUserCreditTransactions.Add(new GibUserCreditTransaction
                    {
                        Id = tr.Id,
                        GibUserCreditAccountId = acc?.Id ?? tr.GibUserCreditAccountId,
                        InvoiceId = invoice.Id,
                        TransactionType = tr.TransactionType,
                        Quantity = tr.Quantity,
                        Description = tr.Description,
                        GibUserCreditAccount = acc,
                        Invoice = invoice
                    });
                }
            }

            return invoice;
        }

        // -------- StaticList --------
        [HttpGet("static/unit")]
        public async Task<IActionResult> GetUnitList([FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetStaticListUnitAsync(ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("static/taxexemption")]
        public async Task<IActionResult> GetTaxExemption([FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetStaticListTaxExemptionReasonsAsync(ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("static/withholding")]
        public async Task<IActionResult> GetWithholding([FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetStaticListWithHoldingAsync(ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        // -------- e-Fatura Outbox --------
        [HttpPost("einvoice/send-json/{id:long}")]
        public async Task<IActionResult> SendEInvoiceJson(
            long id,
            [FromQuery] long userId,
            bool isExport = false,
            [FromQuery(Name = "alias")] string? targetAlias = null,
            CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var inv = await LoadInvoiceAsync(id, ct);
            if (inv == null) return NotFound($"Invoice {id} bulunamadı.");

            if (id == 4)
                isExport = true;

            var res = await _gib.SendEInvoiceJsonAsync(inv, isExport, true, inv.Customer?.TaxNo, targetAlias, ct);

            await LogGibSendInvoiceResultAsync(
                inv,
                res,
                operationName: "SendEInvoiceJson",
                currentUserId: userId,
                ct: ct);

            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpPost("einvoice/send-ubl")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SendEInvoiceUbl(
            [FromForm] SendEInvoiceUblRequest request,
            [FromQuery] long userId,
            CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            if (request.InvoiceFile == null || request.InvoiceFile.Length == 0)
                return BadRequest("UBL dosyası gerekli.");

            var inv = await LoadInvoiceAsync(request.InvoiceId, ct);
            if (inv == null) return NotFound($"Invoice {request.InvoiceId} bulunamadı.");

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
        public async Task<IActionResult> GetEInvoiceOutboxPdf(
            Guid ettn,
            [FromQuery] long userId,
            bool standardXslt = true,
            CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetEInvoiceOutboxPdfAsync(ettn, standardXslt, ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);
            return File(res.Data!, "application/pdf", $"{ettn}.pdf");
        }

        [HttpGet("einvoice/outbox/ubl/{ettn:guid}")]
        public async Task<IActionResult> GetEInvoiceOutboxUbl(
            Guid ettn,
            [FromQuery] long userId,
            bool standardXslt = true,
            CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetEInvoiceOutboxUblAsync(ettn, ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);

            return File(res.Data, "application/xml", $"{ettn}.xml");
        }

        // -------- e-Fatura Inbox --------
        [HttpGet("einvoice/inbox/list")]
        public async Task<IActionResult> GetInboxList(
            DateTime start,
            DateTime end,
            [FromQuery] long userId,
            int pageIndex = 1,
            int pageSize = 100,
            bool isNew = true,
            CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetEInvoiceInboxAsync(start, end, pageIndex, pageSize, isNew, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("einvoice/inbox/pdf/{ettn:guid}")]
        public async Task<IActionResult> GetEInvoiceInboxPdf(
            Guid ettn,
            [FromQuery] long userId,
            bool standardXslt = true,
            CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetEInvoiceInboxPdfAsync(ettn, standardXslt, ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);
            return File(res.Data!, "application/pdf", $"{ettn}.pdf");
        }

        [HttpGet("einvoice/inbox/ubl/{ettn:guid}")]
        public async Task<IActionResult> GetEInvoiceInboxUBL(
            Guid ettn,
            [FromQuery] long userId,
            CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetEInvoiceInboxUblAsync(ettn, ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);
            return File(res.Data!, "application/xml", $"{ettn}.xml");
        }

        [HttpPost("einvoice/response")]
        public async Task<IActionResult> SendInvoiceResponse(
            [FromBody] InvoiceResponseRequest request,
            [FromQuery] long userId,
            CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.SendInvoiceResponseAsync(request, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("einvoice/outbox/list")]
        public async Task<IActionResult> GetOutboxList(
     [FromQuery] long userId,
     [FromQuery] string? start = null,
     [FromQuery] string? end = null,
     [FromQuery] int? integrationYear = null,
     [FromQuery] int? integrationMonth = null,
     [FromQuery] int pageIndex = 1,
     [FromQuery] int pageSize = 200,
     [FromQuery] bool isDesc = true,
     CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            static bool TryParseFlexible(string? s, out DateTime dt)
            {
                dt = default;
                if (string.IsNullOrWhiteSpace(s)) return false;

                s = s.Trim();

                // JS’in gönderdiği yaygın formatlar
                var formats = new[]
                {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm"
        };

                if (DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out dt))
                    return true;

                // son çare
                return DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dt)
                       || DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt);
            }

            var hasStart = TryParseFlexible(start, out var startDt);
            var hasEnd = TryParseFlexible(end, out var endDt);

            if (!hasStart || !hasEnd)
            {
                DateTime calcStart, calcEnd;

                if (integrationYear.HasValue &&
                    integrationMonth.HasValue &&
                    integrationMonth.Value >= 1 && integrationMonth.Value <= 12)
                {
                    var first = new DateTime(integrationYear.Value, integrationMonth.Value, 1);
                    var last = first.AddMonths(1).AddTicks(-1);
                    calcStart = first;
                    calcEnd = last;
                }
                else
                {
                    // fallback: son 30 gün
                    var today = DateTime.Today;
                    calcStart = today.AddDays(-30);
                    calcEnd = today.AddDays(1).AddTicks(-1);
                }

                if (!hasStart) startDt = calcStart;
                if (!hasEnd) endDt = calcEnd;
            }

            var res = await _gib.GetEInvoiceOutboxAsync(startDt, endDt, pageIndex, pageSize, isDesc, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }


        // -------- e-Arşiv --------
        // -------- e-Arşiv --------
        [HttpPost("earchive/send-json/{id:long}")]
        public async Task<IActionResult> SendEArchiveJson(
            long id,
            [FromQuery] long userId,
            CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            // ✅ e-Fatura ile aynı mantık: DB'den yükle
            var inv = await LoadInvoiceAsync(id, ct);
            if (inv == null) return NotFound($"Invoice {id} bulunamadı.");

            // Kontör için mümkünse firma VKN; yoksa fallback
            var firmVkn = await GetFirmTaxNoAsync(userId, ct);
            var kontorVkn = firmVkn ?? inv.Customer?.TaxNo;

            var res = await _gib.SendEArchiveJsonAsync(inv, consumeKontor: true, kontorVkn: kontorVkn, ct);

            await LogGibSendInvoiceResultAsync(
                inv,
                res,
                operationName: "SendEArchiveJson",
                currentUserId: userId,
                ct: ct);

            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }


        [HttpPost("earchive/send-json-raw")]
        public async Task<IActionResult> SendEArchiveJsonRaw(
            [FromBody] JsonElement body,
            [FromQuery] long userId,
            CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.SendEArchiveJsonRawAsync(body, true, null, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("earchive/pdf/{ettn:guid}")]
        public async Task<IActionResult> GetEArchivePdf(
            Guid ettn,
            [FromQuery] long userId,
            bool standardXslt = true,
            CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetEArchivePdfAsync(ettn, standardXslt, ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);
            return File(res.Data!, "application/pdf", $"{ettn}.pdf");
        }

        [HttpPost("earchive/retry-mail/{id:guid}")]
        public async Task<IActionResult> RetryEArchiveMail(
            Guid id,
            [FromQuery] long userId,
            CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.RetryEArchiveMailAsync(id, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpPost("earchive/retry-mail-custom")]
        public async Task<IActionResult> RetryEArchiveMailCustom(
            [FromBody] EArchiveMailRetryRequest req,
            [FromQuery] long userId,
            CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.RetryEArchiveMailToAddressesAsync(req, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        // -------- GIB User --------
        [HttpGet("gibuser/recipient-zip")]
        public async Task<IActionResult> GetGibUserRecipientZip(
            [FromQuery] long userId,
            CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetGibUserRecipientZipAsync(ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);
            return File(res.Data!, "application/zip", "gibuser-recipient.zip");
        }

        // ============================================================
        // ✅ EKLENEN ENDPOINT'LER (mevcut yapıyı bozmadan, sadece ek)
        // ============================================================

        // =========================
        //        KONTÖR
        // =========================
        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance([FromQuery] long userId, [FromQuery] string? companyVkn = null, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var firmVkn = companyVkn ?? await GetFirmTaxNoAsync(userId, ct);
            if (string.IsNullOrWhiteSpace(firmVkn)) return BadRequest("companyVkn bulunamadı.");

            var (remaining, used) = await _gib.GetBalanceAsync(firmVkn, ct);
            return Ok(new { remaining, used });
        }

        [HttpPost("balance/consume")]
        public async Task<IActionResult> ConsumeBalance([FromQuery] long userId, [FromBody] ConsumeBalanceRequest req, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var firmVkn = req.CompanyVkn ?? await GetFirmTaxNoAsync(userId, ct);
            if (string.IsNullOrWhiteSpace(firmVkn)) return BadRequest("companyVkn bulunamadı.");

            try
            {
                var (remaining, used) = await _gib.ConsumeBalanceAsync(firmVkn, req.Amount, req.Reason, ct);
                return Ok(new { remaining, used });
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        // =========================
        //   STATIC LIST (EKSİKLER)
        // =========================
        [HttpGet("static/taxtypecode")]
        public async Task<IActionResult> GetTaxTypeCode([FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetStaticListTaxTypeCodeAsync(ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("static/taxoffice")]
        public async Task<IActionResult> GetTaxOffice([FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetStaticListTaxOfficeAsync(ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("static/country")]
        public async Task<IActionResult> GetCountry([FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetStaticListCountryAsync(ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        // =========================
        //  e-FATURA OUTBOX (EKSİKLER)
        // =========================
        [HttpPut("einvoice/outbox/update-ubl")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateEInvoiceUbl([FromForm] UpdateEInvoiceUblRequest request, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            if (request.InvoiceFile == null || request.InvoiceFile.Length == 0)
                return BadRequest("UBL dosyası gerekli.");

            await using var ms = new System.IO.MemoryStream();
            await request.InvoiceFile.CopyToAsync(ms, ct);
            ms.Position = 0;

            var firmVkn = await GetFirmTaxNoAsync(userId, ct);

            var res = await _gib.UpdateEInvoiceUblAsync(
                request.Ettn,
                ms,
                request.InvoiceFile.FileName,
                request.AppType,
                request.Status,
                request.UseManualInvoiceId,
                request.TargetAlias,
                request.UseFirstAlias,
                request.LocalReferenceId,
                request.CheckLocalReferenceId,
                request.XsltCode,
                consumeKontor: false,
                kontorVkn: firmVkn,
                ct: ct);

            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpPut("einvoice/outbox/update-json/{ettn:guid}")]
        public async Task<IActionResult> UpdateEInvoiceJson([FromRoute] Guid ettn, [FromBody] JsonElement body, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.UpdateEInvoiceJsonAsync(ettn, body, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpPut("einvoice/outbox/statuslist")]
        public async Task<IActionResult> UpdateEInvoiceStatusList([FromBody] UpdateStatusListRequest req, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.UpdateEInvoiceStatusAsync(req.Ids, req.Status, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("einvoice/outbox/status/{ettn:guid}")]
        public async Task<IActionResult> GetEInvoiceOutboxStatus([FromRoute] Guid ettn, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetEInvoiceOutboxStatusAsync(ettn, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("einvoice/outbox/html/{ettn:guid}")]
        public async Task<IActionResult> GetEInvoiceOutboxHtml([FromRoute] Guid ettn, [FromQuery] long userId, bool standardXslt = true, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetEInvoiceOutboxHtmlAsync(ettn, standardXslt, ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);
            return File(res.Data!, "text/html", $"{ettn}.html");
        }

        [HttpGet("einvoice/outbox/with-null-localref")]
        public async Task<IActionResult> GetOutboxInvoicesWithNullLocalReferences(DateTime start, DateTime end, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetOutboxInvoicesWithNullLocalReferencesAsync(start, end, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("einvoice/outbox/reason/{ettn:guid}")]
        public async Task<IActionResult> GetOutboxInvoiceReason([FromRoute] Guid ettn, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetOutboxInvoiceReasonAsync(ettn, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        // =========================
        //  e-FATURA INBOX (EKSİKLER)
        // =========================
        [HttpGet("einvoice/inbox/html/{ettn:guid}")]
        public async Task<IActionResult> GetEInvoiceInboxHtml([FromRoute] Guid ettn, [FromQuery] long userId, bool standardXslt = true, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetEInvoiceInboxHtmlAsync(ettn, standardXslt, ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);
            return File(res.Data!, "text/html", $"{ettn}.html");
        }

        [HttpGet("einvoice/inbox/status/{ettn:guid}")]
        public async Task<IActionResult> GetEInvoiceInboxStatus([FromRoute] Guid ettn, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetEInvoiceInboxStatusAsync(ettn, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpPut("einvoice/response/retry")]
        public async Task<IActionResult> RetryInvoiceResponseList([FromBody] List<Guid> invoiceIds, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.RetryInvoiceResponseListAsync(invoiceIds, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpPut("einvoice/inbox/isnew")]
        public async Task<IActionResult> UpdateInboxInvoiceIsNew([FromBody] List<InboxInvoiceIsNewUpdate> items, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.UpdateInboxInvoiceIsNewAsync(items, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        // =========================
        //   e-ARŞİV (EKSİKLER)
        // =========================
        [HttpPost("earchive/send-ubl")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SendEArchiveUbl([FromForm] SendEArchiveUblRequest request, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            if (request.InvoiceFile == null || request.InvoiceFile.Length == 0)
                return BadRequest("UBL dosyası gerekli.");

            await using var ms = new System.IO.MemoryStream();
            await request.InvoiceFile.CopyToAsync(ms, ct);
            ms.Position = 0;

            var firmVkn = await GetFirmTaxNoAsync(userId, ct);

            var res = await _gib.SendEArchiveUblAsync(
                ms,
                request.InvoiceFile.FileName,
                request.Status,
                request.UseManualInvoiceId,
                request.SendEmail,
                request.EmailAddress,
                request.Prefix,
                request.LocalReferenceId,
                request.CheckLocalReferenceId,
                request.XsltCode,
                request.AllowOldEArsivCustomer,
                consumeKontor: true,
                kontorVkn: firmVkn,
                ct: ct);

            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpPut("earchive/update-ubl")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateEArchiveUbl([FromForm] UpdateEArchiveUblRequest request, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            if (request.InvoiceFile == null || request.InvoiceFile.Length == 0)
                return BadRequest("UBL dosyası gerekli.");

            await using var ms = new System.IO.MemoryStream();
            await request.InvoiceFile.CopyToAsync(ms, ct);
            ms.Position = 0;

            var res = await _gib.UpdateEArchiveUblAsync(
                request.Id,
                ms,
                request.InvoiceFile.FileName,
                request.Status,
                request.SendEmail,
                request.EmailAddress,
                request.LocalReferenceId,
                request.CheckLocalReferenceId,
                request.XsltCode,
                request.AllowOldEArsivCustomer,
                ct);

            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("earchive/status/{ettn:guid}")]
        public async Task<IActionResult> GetEArchiveStatus([FromRoute] Guid ettn, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetEArchiveStatusAsync(ettn, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("earchive/html/{ettn:guid}")]
        public async Task<IActionResult> GetEArchiveHtml([FromRoute] Guid ettn, [FromQuery] long userId, bool standardXslt = true, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetEArchiveHtmlAsync(ettn, standardXslt, ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);
            return File(res.Data!, "text/html", $"{ettn}.html");
        }

        [HttpGet("earchive/ubl/{ettn:guid}")]
        public async Task<IActionResult> GetEArchiveUbl([FromRoute] Guid ettn, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetEArchiveUblAsync(ettn, ct);
            if (!res.Ok) return StatusCode(res.StatusCode, res.Error);
            return File(res.Data!, "application/xml", $"{ettn}.xml");
        }

        [HttpGet("earchive/with-null-localref")]
        public async Task<IActionResult> GetEArchiveWithNullLocalReferences(DateTime start, DateTime end, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetEArchiveWithNullLocalReferencesAsync(start, end, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpPut("earchive/cancel")]
        public async Task<IActionResult> CancelEArchive([FromBody] List<Guid> ids, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.CancelEArchiveAsync(ids, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        // =========================
        //   e-İRSALİYE (EKSİKLER)
        // =========================
        [HttpPost("edespatch/send-ubl/{invoiceId:long}")]
        public async Task<IActionResult> SendDespatchUbl([FromRoute] long invoiceId, [FromQuery] long userId, [FromQuery] string? targetAlias = null, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var inv = await LoadInvoiceAsync(invoiceId, ct);
            if (inv == null) return NotFound($"Invoice {invoiceId} bulunamadı.");

            var firmVkn = await GetFirmTaxNoAsync(userId, ct);

            var res = await _gib.SendDespatchUblAsync(inv, targetAlias, consumeKontor: true, kontorVkn: firmVkn, ct: ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpPut("edespatch/outbox/statuslist")]
        public async Task<IActionResult> UpdateDespatchStatusList([FromBody] UpdateStatusListRequest req, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.UpdateDespatchStatusListAsync(req.Ids, req.Status, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpGet("edespatch/inbox/list")]
        public async Task<IActionResult> GetDespatchInbox(DateTime start, DateTime end, [FromQuery] long userId, int pageIndex = 1, int pageSize = 100, bool isDesc = true, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetDespatchInboxAsync(start, end, pageIndex, pageSize, isDesc, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        // =========================
        //     e-DEFTER (EKSİKLER)
        // =========================
        [HttpGet("edefter/period/list")]
        public async Task<IActionResult> GetEDefterPeriodList([FromQuery] long userId, int pageIndex = 1, int pageSize = 50, bool isDesc = true, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.GetEDefterPeriodListAsync(pageIndex, pageSize, isDesc, ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpPost("edefter/period")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PostEDefter([FromForm] PostEDefterFormRequest req, [FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            if (req.ZipFile == null || req.ZipFile.Length == 0)
                return BadRequest("ZipFile gerekli.");

            var model = new PostEDefterRequest
            {
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                SplitSize = req.SplitSize,
                TimeStamp = req.TimeStamp,
                WithoutTaxDetail = req.WithoutTaxDetail
            };

            await using var ms = new System.IO.MemoryStream();
            await req.ZipFile.CopyToAsync(ms, ct);
            ms.Position = 0;

            var firmVkn = await GetFirmTaxNoAsync(userId, ct);

            var res = await _gib.PostEDefterAsync(model, ms, req.ZipFile.FileName, consumeKontor: true, kontorVkn: firmVkn, ct: ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        [HttpPost("edefter/send-letter-patents")]
        public async Task<IActionResult> SendLetterPatentsToGib([FromQuery] long userId, CancellationToken ct = default)
        {
            var applyResult = await ApplyGibFirmForUserAsync(userId, ct);
            if (applyResult != null) return applyResult;

            var res = await _gib.SendLetterPatentsToGibAsync(ct);
            return res.Ok ? Ok(res.Data) : StatusCode(res.StatusCode, res.Error);
        }

        private async Task LogGibSendInvoiceResultAsync(
          Invoice invoice,
          HttpResult<object> res,
          string operationName,
          long? currentUserId,
          CancellationToken ct)
        {
            var now = DateTimeOffset.UtcNow;
            var ownerUserId = currentUserId ?? invoice.UserId;

            // --------- OPERASYON LOGU ---------
            var log = new GibInvoiceOperationLog
            {
                InvoiceId = invoice.Id,
                Invoice = null, // büyük grafiği bağlamaya gerek yok

                OperationName = string.IsNullOrWhiteSpace(operationName)
                    ? "Unknown"
                    : operationName,

                IsSuccess = res.Ok,

                UserId = ownerUserId > 0 ? ownerUserId : null,
                CreatedAt = now.UtcDateTime,

                // 🔹 NULL GÖNDERME!
                ExternalInvoiceId = string.Empty,
                InvoiceNumber = string.Empty,
                ErrorCode = res.StatusCode.ToString(), // 200, 422 vs.
                ErrorMessage = string.Empty,
                RawResponseJson = string.Empty
            };

            // Bu çağrıda oluşacak kontör hareketi (başarılıysa)
            GibUserCreditTransaction? creditTrx = null;

            // ==============================
            //   SUCCESS (HTTP 200)
            // ==============================
            if (res.Ok && res.Data is not null)
            {
                // Data genelde JsonElement (Id, InvoiceNumber içeriyor)
                if (res.Data is JsonElement elem)
                {
                    if (elem.TryGetProperty("Id", out var idProp) &&
                        idProp.ValueKind == JsonValueKind.String)
                    {
                        log.ExternalInvoiceId = idProp.GetString() ?? string.Empty;
                    }

                    if (elem.TryGetProperty("InvoiceNumber", out var invNoProp) &&
                        invNoProp.ValueKind == JsonValueKind.String)
                    {
                        log.InvoiceNumber = invNoProp.GetString() ?? string.Empty;
                    }

                    log.RawResponseJson = elem.GetRawText();
                }
                else
                {
                    // Tip beklediğimiz gibi değilse yine de JSON’a çevirip saklayalım
                    log.RawResponseJson = JsonSerializer.Serialize(res.Data);
                }

                // --------- KONTÖR HESABI (GibUserCreditAccount) & HAREKETİ (GibUserCreditTransaction) ---------
                if (ownerUserId > 0)
                {
                    // Bu kullanıcıya ait aktif kontör hesabı
                    var creditAccount = await _db.Set<GibUserCreditAccount>()
                        .FirstOrDefaultAsync(a => a.UserId == ownerUserId && a.IsActive, ct);

                    if (creditAccount != null)
                    {
                        // UsedCredits +1
                        creditAccount.UsedCredits += 1;
                        creditAccount.UpdatedAt = now;
                        creditAccount.UpdatedByUserId = ownerUserId;
                        creditAccount.IsActive = true;
                        // RowVersion: DB tarafında rowversion/timestamp üretildiği için burada set etmiyoruz.

                        // Yeni kontör hareketi
                        creditTrx = new GibUserCreditTransaction
                        {
                            GibUserCreditAccountId = creditAccount.Id,
                            GibUserCreditAccount = creditAccount,

                            InvoiceId = invoice.Id,
                            Invoice = null, // büyük grafiği bağlamaya gerek yok

                            TransactionType = GibCreditTransactionType.Usage,
                            Quantity = 1,
                            Description = $"Invoice {invoice.InvoiceNo ?? invoice.Id.ToString()} için 1 kontör kullanıldı.",

                            // BaseEntity alanları
                            UserId = ownerUserId,
                            IsActive = true,
                            DeleteDate = null,
                            DeletedByUserId = null,
                            CreatedAt = now,
                            UpdatedAt = now,
                            CreatedByUserId = ownerUserId,
                            UpdatedByUserId = ownerUserId,
                            // RowVersion: yeni kayıtta DB rowversion üreteceği için null bırakıyoruz
                            RowVersion = null
                        };

                        _db.Set<GibUserCreditTransaction>().Add(creditTrx);
                    }
                    // Eğer kontör hesabı yoksa şimdilik sessiz geçiyoruz;
                    // istersen burada exception/log ekleyebilirsin.
                }
            }
            // ==============================
            //   HATA (ör: HTTP 422)
            // ==============================
            else
            {
                log.IsSuccess = false;
                log.ErrorCode = res.StatusCode.ToString(); // zaten yukarıda set edildi, tekrar yazmak sorun değil

                var raw = res.Error ?? string.Empty;
                log.RawResponseJson = raw;

                try
                {
                    if (!string.IsNullOrWhiteSpace(raw))
                    {
                        using var doc = JsonDocument.Parse(raw);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("Error", out var err))
                        {
                            string? detail = null;
                            if (err.TryGetProperty("detail", out var detailProp) &&
                                detailProp.ValueKind == JsonValueKind.String)
                            {
                                detail = detailProp.GetString();
                            }

                            string? title = null;
                            if (err.TryGetProperty("title", out var titleProp) &&
                                titleProp.ValueKind == JsonValueKind.String)
                            {
                                title = titleProp.GetString();
                            }

                            log.ErrorMessage = !string.IsNullOrWhiteSpace(detail)
                                ? detail!
                                : (!string.IsNullOrWhiteSpace(title) ? title! : raw);
                        }
                        else
                        {
                            log.ErrorMessage = raw;
                        }
                    }
                }
                catch
                {
                    // JSON parse edilemezse ham mesajı yazalım
                    log.ErrorMessage = raw;
                }
            }

            // Her durumda operasyon log’u kaydet
            _db.Set<GibInvoiceOperationLog>().Add(log);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                // burada gerçek loglama yapman daha sağlıklı olur
                // örn: _logger.LogError(ex, "GibInvoiceOperationLog insert hatası");
                ex.ToString();
            }
        }
    }
}
