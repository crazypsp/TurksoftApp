using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TurkSoft.Data.GibData;
using TurkSoft.Entities.GIBEntityDB;
using TurkSoft.Service.Interface;

namespace TurkSoft.Service.Manager
{
    /// <summary>
    /// GIB veritabanı (GibAppDbContext) üzerinde çalışan generic servis.
    /// Id tipi (TKey) entity sınıflarına göre belirlenir (int/long/Guid).
    /// IsActive özelliği varsa soft delete uygular.
    /// </summary>
    public class EntityGibManager<TEntity, TKey> : IEntityGibService<TEntity, TKey>
        where TEntity : class
    {
        private readonly GibAppDbContext _db;
        private readonly DbSet<TEntity> _set;

        private static readonly PropertyInfo IdProp = typeof(TEntity).GetProperty("Id")
            ?? throw new InvalidOperationException($"'{typeof(TEntity).Name}' üzerinde 'Id' alanı yok.");

        private static readonly PropertyInfo? IsActiveProp = typeof(TEntity).GetProperty("IsActive");

        public EntityGibManager(GibAppDbContext db)
        {
            _db = db;
            _set = _db.Set<TEntity>();
        }

        // ======================================================
        // Public API
        // ======================================================

        public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
        {
            var query = _set.AsNoTracking().AsQueryable();
            query = ApplyIsActiveFilterIfExists(query);
            var predicate = BuildIdEqualsExpression(id);
            return await query.FirstOrDefaultAsync(predicate, ct);
        }

        public async Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
        {
            var query = _set.AsNoTracking().AsQueryable();
            query = ApplyIsActiveFilterIfExists(query);
            return await query.ToListAsync(ct);
        }

        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default)
        {
            // RequestAborted’a yakalanmamak için EF tarafında iptal edilemeyen token
            var token = CancellationToken.None;

            // Yoğun grafikler (lookup + ekleme) için zaman aşımını yükselt
            var prevTimeout = _db.Database.GetCommandTimeout();
            _db.Database.SetCommandTimeout(TimeSpan.FromSeconds(120));

            try
            {
                if (entity is Invoice inv)
                {
                    await FixInvoiceGraphAsync(inv, token);

                    // Fail-fast güvenliği: Phone NOT NULL kırılmasın
                    if (inv.Customer != null && string.IsNullOrWhiteSpace(inv.Customer.Phone))
                    {
                        NormalizeCustomer(inv.Customer);
                    }
                }

                await _set.AddAsync(entity, token);
                await _db.SaveChangesAsync(token);

                return entity;
            }
            catch (OperationCanceledException oce)
            {
                throw new Exception(
                    "Veri kaydı iptal edildi (OperationCanceled). Genellikle istek iptali veya SQL zaman aşımı/kilit kaynaklıdır.", oce);
            }
            catch (DbUpdateException due)
            {
                var root = due.InnerException?.Message ?? due.Message;
                throw new Exception("DB update sırasında hata: " + root, due);
            }
            finally
            {
                _db.Database.SetCommandTimeout(prevTimeout);
            }
        }

        public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken ct = default)
        {
            _set.Update(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<bool> DeleteAsync(TKey id, CancellationToken ct = default)
        {
            var entity = await _set.FirstOrDefaultAsync(BuildIdEqualsExpression(id), ct);
            if (entity == null) return false;

            if (IsActiveProp != null && IsActiveProp.PropertyType == typeof(bool))
            {
                // Şemanı koruyorum (true); gerekiyorsa false yap.
                IsActiveProp.SetValue(entity, true);
            }
            else
            {
                _set.Remove(entity);
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }

        // ======================================================
        // Helpers (Generic)
        // ======================================================

        private static Expression<Func<TEntity, bool>> BuildIdEqualsExpression(TKey id)
        {
            var param = Expression.Parameter(typeof(TEntity), "e");
            var left = Expression.Property(param, IdProp);

            object? idValue;
            try
            {
                idValue = Convert.ChangeType(id, IdProp.PropertyType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Id dönüşümü başarısız: {typeof(TKey).Name} -> {IdProp.PropertyType.Name}", ex);
            }

            var right = Expression.Constant(idValue, IdProp.PropertyType);
            var body = Expression.Equal(left, right);
            return Expression.Lambda<Func<TEntity, bool>>(body, param);
        }

        private static IQueryable<TEntity> ApplyIsActiveFilterIfExists(IQueryable<TEntity> query)
        {
            if (IsActiveProp == null || IsActiveProp.PropertyType != typeof(bool))
                return query;

            var param = Expression.Parameter(typeof(TEntity), "e");
            var left = Expression.Property(param, IsActiveProp);
            var body = Expression.Equal(left, Expression.Constant(true));
            var lambda = Expression.Lambda<Func<TEntity, bool>>(body, param);
            return query.Where(lambda);
        }

        private static object? ChangeType(object? value, Type targetType)
        {
            var t = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (value == null) return t.IsValueType ? Activator.CreateInstance(t) : null;
            if (t.IsEnum) return Enum.Parse(t, value.ToString()!);
            return Convert.ChangeType(value, t);
        }

        private static bool IsNonDefault(object? value)
        {
            if (value == null) return false;
            var t = value.GetType();
            var def = t.IsValueType ? Activator.CreateInstance(t) : null;
            return !Equals(value, def);
        }

        private static CancellationToken SafeToken(CancellationToken ct) => CancellationToken.None;
        private static long ToInt64(object v) { try { return Convert.ToInt64(v); } catch { return 0; } }

        private Type GetPkType(Type entityType)
            => _db.Model.FindEntityType(entityType)!.FindPrimaryKey()!.Properties[0].ClrType;

        private object ReadIdValue(object entity)
            => entity.GetType().GetProperty("Id")!.GetValue(entity)!;

        // ======================================================
        // Normalizers (NOT NULL güvenliği)
        // ======================================================

        private static void NormalizePaymentType(PaymentType pt)
        {
            pt.Name = string.IsNullOrWhiteSpace(pt.Name) ? "NAKIT" : pt.Name;
            pt.Desc = string.IsNullOrWhiteSpace(pt.Desc) ? pt.Name : pt.Desc;
            if (pt.CreatedAt == default) pt.CreatedAt = DateTime.UtcNow;
            pt.UpdatedAt = DateTime.UtcNow;
        }

        private static void NormalizeBank(Bank b)
        {
            b.Name = string.IsNullOrWhiteSpace(b.Name) ? "Ziraat Bankası" : b.Name;
            b.SwiftCode = string.IsNullOrWhiteSpace(b.SwiftCode) ? "TCZBTR2A" : b.SwiftCode;
            b.Country = string.IsNullOrWhiteSpace(b.Country) ? "TR" : b.Country;
            b.City = string.IsNullOrWhiteSpace(b.City) ? "Ankara" : b.City;
            if (b.CreatedAt == default) b.CreatedAt = DateTime.UtcNow;
            b.UpdatedAt = DateTime.UtcNow;
        }

        private static void NormalizePaymentAccount(PaymentAccount pa, string defaultCurrency)
        {
            pa.Name = string.IsNullOrWhiteSpace(pa.Name) ? "KASA" : pa.Name;
            pa.Desc = string.IsNullOrWhiteSpace(pa.Desc) ? "Varsayılan Kasa" : pa.Desc; // NOT NULL
            pa.AccountNo = string.IsNullOrWhiteSpace(pa.AccountNo) ? "0001" : pa.AccountNo;
            pa.Iban = string.IsNullOrWhiteSpace(pa.Iban) ? "TR000000000000000000000000" : pa.Iban;
            pa.Currency = string.IsNullOrWhiteSpace(pa.Currency) ? (string.IsNullOrWhiteSpace(defaultCurrency) ? "TRY" : defaultCurrency) : pa.Currency;

            if (pa.CreatedAt == default) pa.CreatedAt = DateTime.UtcNow;
            pa.UpdatedAt = DateTime.UtcNow;
        }

        private static void NormalizeBrand(Brand b)
        {
            b.Name = string.IsNullOrWhiteSpace(b.Name) ? "GENEL" : b.Name;
            b.Country = string.IsNullOrWhiteSpace(b.Country) ? "TR" : b.Country;
            if (b.CreatedAt == default) b.CreatedAt = DateTime.UtcNow;
            b.UpdatedAt = DateTime.UtcNow;
        }

        private static void NormalizeCustomer(Customer c)
        {
            c.Name  = string.IsNullOrWhiteSpace(c.Name)  ? "GENEL MÜŞTERİ" : c.Name;
            c.Phone = string.IsNullOrWhiteSpace(c.Phone) ? "-" : c.Phone; // Phone NOT NULL güvenliği
            if (c.CreatedAt == default) c.CreatedAt = DateTime.UtcNow;
            c.UpdatedAt = DateTime.UtcNow;
        }

        // ======================================================
        // Dynamic Find / Ensure
        // ======================================================

        private async Task<object?> FindByIdAsync(Type entityType, object id, CancellationToken ct)
        {
            var token = SafeToken(ct);
            var pk = GetPkType(entityType);
            var keyObj = ChangeType(id, pk)!;
            var found = await _db.FindAsync(entityType, new object[] { keyObj }, token);
            return found;
        }

        private async Task<T?> FindByIdAsync<T>(object id, CancellationToken ct) where T : class
            => (await FindByIdAsync(typeof(T), id, ct)) as T;

        private async Task<T?> FindByStringPropertyAsync<T>(string propName, string? value, CancellationToken ct)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var token = SafeToken(ct);
            var type = typeof(T);

            var pi = type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pi == null || pi.PropertyType != typeof(string)) return null;

            var p = Expression.Parameter(type, "x");
            var body = Expression.Equal(Expression.Property(p, pi), Expression.Constant(value));
            var lambda = Expression.Lambda<Func<T, bool>>(body, p);

            return await _db.Set<T>().AsNoTracking().FirstOrDefaultAsync(lambda, token);
        }

        private async Task<T?> FindLookupAsync<T>(object? probe, CancellationToken ct) where T : class
        {
            if (probe == null) return null;

            // 1) Id
            var idProp = probe.GetType().GetProperty("Id");
            var idVal = idProp?.GetValue(probe);
            if (IsNonDefault(idVal))
            {
                var found = await FindByIdAsync<T>(idVal!, ct);
                if (found != null) { _db.Attach(found); _db.Entry(found).State = EntityState.Unchanged; return found; }
            }

            // 2) Code / ShortName / Name / SwiftCode
            var code = probe.GetType().GetProperty("Code")?.GetValue(probe)?.ToString();
            if (!string.IsNullOrWhiteSpace(code))
            {
                var found = await FindByStringPropertyAsync<T>("Code", code, ct);
                if (found != null) { _db.Attach(found); _db.Entry(found).State = EntityState.Unchanged; return found; }
            }

            var shortName = probe.GetType().GetProperty("ShortName")?.GetValue(probe)?.ToString();
            if (!string.IsNullOrWhiteSpace(shortName))
            {
                var found = await FindByStringPropertyAsync<T>("ShortName", shortName, ct);
                if (found != null) { _db.Attach(found); _db.Entry(found).State = EntityState.Unchanged; return found; }
            }

            var name = probe.GetType().GetProperty("Name")?.GetValue(probe)?.ToString();
            if (!string.IsNullOrWhiteSpace(name))
            {
                var found = await FindByStringPropertyAsync<T>("Name", name, ct);
                if (found != null) { _db.Attach(found); _db.Entry(found).State = EntityState.Unchanged; return found; }
            }

            var swift = probe.GetType().GetProperty("SwiftCode")?.GetValue(probe)?.ToString();
            if (!string.IsNullOrWhiteSpace(swift))
            {
                var found = await FindByStringPropertyAsync<T>("SwiftCode", swift, ct);
                if (found != null) { _db.Attach(found); _db.Entry(found).State = EntityState.Unchanged; return found; }
            }

            return null;
        }

        private async Task<T> EnsureLookupAsync<T>(IDictionary<string, string?> candidates, CancellationToken ct)
            where T : class, new()
        {
            // 1) Önce bul
            foreach (var key in new[] { "Code", "ShortName", "Name", "SwiftCode" })
            {
                if (!candidates.TryGetValue(key, out var val) || string.IsNullOrWhiteSpace(val)) continue;
                var found = await FindByStringPropertyAsync<T>(key, val, ct);
                if (found != null)
                {
                    _db.Attach(found);
                    _db.Entry(found).State = EntityState.Unchanged;
                    return found;
                }
            }

            // 2) Yoksa oluştur
            var now = DateTime.UtcNow;
            var obj = new T();

            void TrySetStr(string name, string? val)
            {
                if (string.IsNullOrWhiteSpace(val)) return;
                var p = typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null && p.CanWrite && p.PropertyType == typeof(string))
                    p.SetValue(obj, val);
            }

            TrySetStr("Code", candidates.TryGetValue("Code", out var c) ? c : null);
            TrySetStr("ShortName", candidates.TryGetValue("ShortName", out var s) ? s : null);
            TrySetStr("Name", candidates.TryGetValue("Name", out var n) ? n : null);
            TrySetStr("SwiftCode", candidates.TryGetValue("SwiftCode", out var sw) ? sw : null);
            TrySetStr("Currency", candidates.TryGetValue("Currency", out var cur) ? cur : null);
            TrySetStr("AccountNo", candidates.TryGetValue("AccountNo", out var acc) ? acc : null);
            TrySetStr("Iban", candidates.TryGetValue("Iban", out var iban) ? iban : null);
            TrySetStr("Desc", candidates.TryGetValue("Desc", out var desc) ? desc : null);
            TrySetStr("Country", candidates.TryGetValue("Country", out var country) ? country : null);

            // NOT NULL alanlar için normalize
            if (obj is PaymentAccount paObj)
            {
                NormalizePaymentAccount(paObj, paObj.Currency ?? "TRY");
            }
            if (obj is Bank bankObj)
            {
                NormalizeBank(bankObj);
            }
            if (obj is PaymentType ptObj)
            {
                NormalizePaymentType(ptObj);
            }
            if (obj is Brand brObj)
            {
                NormalizeBrand(brObj);
            }

            typeof(T).GetProperty("CreatedAt")?.SetValue(obj, now);
            typeof(T).GetProperty("UpdatedAt")?.SetValue(obj, now);

            _db.Set<T>().Add(obj); // SaveChanges dışarıda
            return obj;
        }

        private async Task AttachLookupByFkOrNavOrEnsure<TLookup>(
            object owner, string fkName, string navName,
            Func<object?, IDictionary<string, string?>>? toCandidates,
            CancellationToken ct)
            where TLookup : class, new()
        {
            var fkProp = owner.GetType().GetProperty(fkName);
            var navProp = owner.GetType().GetProperty(navName);
            if (fkProp == null || navProp == null) return;

            var fkVal = fkProp.GetValue(owner);
            var navVal = navProp.GetValue(owner);

            // 1) Id var → bul; yoksa nav’dan ensure et (FK sıfırlanır)
            if (IsNonDefault(fkVal))
            {
                var found = await FindByIdAsync<TLookup>(fkVal!, ct);
                if (found != null)
                {
                    _db.Attach(found);
                    _db.Entry(found).State = EntityState.Unchanged;
                    navProp.SetValue(owner, null);
                    return;
                }

                if (navVal != null && toCandidates != null)
                {
                    var ensured = await EnsureLookupAsync<TLookup>(toCandidates(navVal), ct);
                    navProp.SetValue(owner, ensured);
                    fkProp.SetValue(owner, null); // EF doldurur
                    return;
                }

                // Geçersiz FK → 0'a çek ki conflict olmasın
                fkProp.SetValue(owner, Activator.CreateInstance(fkProp.PropertyType));
            }

            // 2) Id yok, Nav var → bul/oluştur
            if (navVal != null)
            {
                var found = await FindLookupAsync<TLookup>(navVal, ct);
                if (found != null)
                {
                    _db.Attach(found);
                    _db.Entry(found).State = EntityState.Unchanged;
                    return; // EF FK’yi doldurur
                }

                if (toCandidates != null)
                {
                    var ensured = await EnsureLookupAsync<TLookup>(toCandidates(navVal), ct);
                    navProp.SetValue(owner, ensured);
                    return;
                }
            }
        }

        // ======================================================
        // Default helpers
        // ======================================================

        private async Task<Bank> EnsureDefaultBankAsync(CancellationToken ct)
        {
            var probe = new { Name = "Ziraat Bankası", SwiftCode = "TCZBTR2A" };
            var found = await FindLookupAsync<Bank>(probe, ct);
            if (found != null) return found;

            var ensured = await EnsureLookupAsync<Bank>(new Dictionary<string, string?>
            {
                ["Name"] = "Ziraat Bankası",
                ["SwiftCode"] = "TCZBTR2A",
                ["Country"] = "TR",
                ["City"] = "Ankara"
            }, ct);

            return ensured;
        }

        private async Task<Brand> EnsureDefaultBrandAsync(CancellationToken ct)
        {
            var probe = new { Name = "GENEL", Country = "TR" };
            var found = await FindLookupAsync<Brand>(probe, ct);
            if (found != null) return found;

            var ensured = await EnsureLookupAsync<Brand>(new Dictionary<string, string?>
            {
                ["Name"] = "GENEL",
                ["Country"] = "TR"
            }, ct);

            return ensured;
        }

        // ======================================================
        // Invoice Graph Fixer
        // ======================================================

        private async Task FixInvoiceGraphAsync(Invoice inv, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            // ---- CUSTOMER: Id varsa mevcut müşteriyi bağla; yoksa yeni müşteriyi normalize et
            try
            {
                if (inv.CustomerId > 0)
                {
                    var existing = await FindByIdAsync<Customer>(inv.CustomerId, ct);
                    if (existing == null)
                        throw new Exception($"CustomerId {inv.CustomerId} bulunamadı.");

                    _db.Attach(existing);
                    _db.Entry(existing).State = EntityState.Unchanged;
                    inv.Customer = existing; // yeni kayıt denemesini engeller
                }
                else if (inv.Customer != null)
                {
                    NormalizeCustomer(inv.Customer); // Phone dahil zorunlu alanları garanti et

                    // döngüleri kır + timestamp
                    inv.Customer.Invoices = null;
                    inv.Customer.Addresses = null;
                    inv.Customer.CustomersGroups = null;
                    if (inv.Customer.CreatedAt == default) inv.Customer.CreatedAt = now;
                    inv.Customer.UpdatedAt = now;
                }
            }
            catch (MissingMemberException)
            {
                // Eğer Invoice tipinizde CustomerId yoksa sadece normalize/temizle
                if (inv.Customer != null)
                {
                    NormalizeCustomer(inv.Customer);
                    inv.Customer.Invoices = null;
                    inv.Customer.Addresses = null;
                    inv.Customer.CustomersGroups = null;
                    if (inv.Customer.CreatedAt == default) inv.Customer.CreatedAt = now;
                    inv.Customer.UpdatedAt = now;
                }
            }

            // ---- Backrefs temizle (döngü kır)
            foreach (var it in inv.InvoicesItems ?? Enumerable.Empty<InvoicesItem>()) it.Invoice = null;
            foreach (var tx in inv.InvoicesTaxes ?? Enumerable.Empty<InvoicesTax>()) tx.Invoice = null;
            foreach (var sp in inv.ServicesProviders ?? Enumerable.Empty<ServicesProvider>()) sp.Invoice = null;
            foreach (var sg in inv.SgkRecords ?? Enumerable.Empty<Sgk>()) sg.Invoice = null;
            foreach (var p in inv.InvoicesPayments ?? Enumerable.Empty<InvoicesPayment>()) p.Invoice = null;
            foreach (var r in inv.Returns ?? Enumerable.Empty<Returns>()) r.Invoice = null;
            foreach (var t in inv.Tourists ?? Enumerable.Empty<Tourist>()) t.Invoice = null;

            // ---- Items: Unit + Brand çözümleme
            if (inv.InvoicesItems != null)
            {
                foreach (var li in inv.InvoicesItems)
                {
                    if (li.CreatedAt == default) li.CreatedAt = now;
                    li.UpdatedAt = now;

                    var it = li.Item;
                    if (it != null)
                    {
                        if (it.CreatedAt == default) it.CreatedAt = now;
                        it.UpdatedAt = now;

                        // --- UNIT
                        var unitPi = it.GetType().GetProperty("Unit");
                        var unitIdPi = it.GetType().GetProperty("UnitId");
                        var unitObj = unitPi?.GetValue(it);
                        var unitIdVal = unitIdPi?.GetValue(it);

                        // Adaylar
                        string? unitCode = it.GetType().GetProperty("UnitCode")?.GetValue(it)?.ToString();
                        string? unitShort = it.GetType().GetProperty("UnitShortName")?.GetValue(it)?.ToString();
                        string? unitName = it.GetType().GetProperty("UnitName")?.GetValue(it)?.ToString();

                        if (unitObj != null)
                        {
                            unitCode ??= unitObj.GetType().GetProperty("Code")?.GetValue(unitObj)?.ToString();
                            unitShort ??= unitObj.GetType().GetProperty("ShortName")?.GetValue(unitObj)?.ToString();
                            unitName ??= unitObj.GetType().GetProperty("Name")?.GetValue(unitObj)?.ToString();
                        }

                        unitShort ??= "C62";
                        unitName ??= unitShort == "C62" ? "ADET" : unitShort;

                        var unitCandidates = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["Code"] = unitCode,
                            ["ShortName"] = unitShort,
                            ["Name"] = unitName
                        };

                        Unit ensuredUnit;
                        if (unitIdVal != null && ToInt64(unitIdVal) != 0L)
                        {
                            var existing = await FindByIdAsync<Unit>(unitIdVal!, ct);
                            ensuredUnit = existing ?? await EnsureLookupAsync<Unit>(unitCandidates, ct);
                        }
                        else
                        {
                            ensuredUnit = await EnsureLookupAsync<Unit>(unitCandidates, ct);
                        }
                        unitPi?.SetValue(it, ensuredUnit);

                        // --- BRAND
                        var brandPi = it.GetType().GetProperty("Brand");
                        var brandIdPi = it.GetType().GetProperty("BrandId");
                        var brandObj = brandPi?.GetValue(it);
                        var brandIdVal = brandIdPi?.GetValue(it);

                        var brandCandidates = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["Name"] = brandObj?.GetType().GetProperty("Name")?.GetValue(brandObj)?.ToString(),
                            ["Code"] = brandObj?.GetType().GetProperty("Code")?.GetValue(brandObj)?.ToString(),
                            ["Country"] = brandObj?.GetType().GetProperty("Country")?.GetValue(brandObj)?.ToString() ?? "TR"
                        };

                        Brand ensuredBrand;

                        // 1) Geçerli BrandId varsa, bul; yoksa sıfırla ki conflict olmasın
                        if (brandIdVal != null && ToInt64(brandIdVal) != 0L)
                        {
                            var existingBrand = await FindByIdAsync<Brand>(brandIdVal!, ct);
                            if (existingBrand != null)
                            {
                                ensuredBrand = existingBrand;
                            }
                            else
                            {
                                // geçersiz FK → conflict önlemek için 0’a çek
                                brandIdPi?.SetValue(it, Activator.CreateInstance(brandIdPi.PropertyType));
                                // nav’dan veya default’tan ilerle
                                if (brandObj != null)
                                {
                                    var found = await FindLookupAsync<Brand>(brandObj, ct);
                                    ensuredBrand = found ?? await EnsureLookupAsync<Brand>(brandCandidates, ct);
                                }
                                else
                                {
                                    ensuredBrand = await EnsureDefaultBrandAsync(ct);
                                }
                            }
                        }
                        else
                        {
                            // 2) Brand nav varsa bul/oluştur, yoksa default
                            if (brandObj != null)
                            {
                                var found = await FindLookupAsync<Brand>(brandObj, ct);
                                ensuredBrand = found ?? await EnsureLookupAsync<Brand>(brandCandidates, ct);
                            }
                            else
                            {
                                ensuredBrand = await EnsureDefaultBrandAsync(ct);
                            }
                        }

                        NormalizeBrand(ensuredBrand);
                        brandPi?.SetValue(it, ensuredBrand);

                        // — Item tali koleksiyonlar (döngüleri kır)
                        it.GetType().GetProperty("ItemsCategories")?.SetValue(it, null);
                        it.GetType().GetProperty("ItemsDiscounts")?.SetValue(it, null);
                        it.GetType().GetProperty("Identifiers")?.SetValue(it, null);
                    }
                }
            }

            // ---- Taxes
            foreach (var tx in inv.InvoicesTaxes ?? Enumerable.Empty<InvoicesTax>())
            {
                if (tx.CreatedAt == default) tx.CreatedAt = now;
                tx.UpdatedAt = now;
            }

            // ---- Payments (PaymentType / PaymentAccount / Bank)
            if (inv.InvoicesPayments != null)
            {
                foreach (var link in inv.InvoicesPayments)
                {
                    if (link.CreatedAt == default) link.CreatedAt = now;
                    link.UpdatedAt = now;

                    var p = link.Payment;
                    if (p != null)
                    {
                        if (p.CreatedAt == default) p.CreatedAt = now;
                        p.UpdatedAt = now;

                        // PaymentType
                        await AttachLookupByFkOrNavOrEnsure<PaymentType>(
                            p,
                            "PaymentTypeId",
                            "PaymentType",
                            nav => new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["Name"] = nav?.GetType().GetProperty("Name")?.GetValue(nav)?.ToString() ?? "NAKIT",
                                ["Code"] = nav?.GetType().GetProperty("Code")?.GetValue(nav)?.ToString()
                            },
                            ct);

                        if (p.PaymentType != null) NormalizePaymentType(p.PaymentType);

                        // PaymentAccount
                        await AttachLookupByFkOrNavOrEnsure<PaymentAccount>(
                            p,
                            "PaymentAccountId",
                            "PaymentAccount",
                            nav => new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["Name"] = nav?.GetType().GetProperty("Name")?.GetValue(nav)?.ToString() ?? "KASA",
                                ["Code"] = nav?.GetType().GetProperty("Code")?.GetValue(nav)?.ToString(),
                                ["Desc"] = nav?.GetType().GetProperty("Desc")?.GetValue(nav)?.ToString(),
                                ["AccountNo"] = nav?.GetType().GetProperty("AccountNo")?.GetValue(nav)?.ToString(),
                                ["Iban"] = nav?.GetType().GetProperty("Iban")?.GetValue(nav)?.ToString(),
                                ["Currency"] = nav?.GetType().GetProperty("Currency")?.GetValue(nav)?.ToString()
                            },
                            ct);

                        var pa = p.PaymentAccount;
                        if (pa != null)
                        {
                            // 1) BankId verilmişse doğrula, yoksa sıfırla
                            if (pa.BankId != 0)
                            {
                                var bank = await FindByIdAsync<Bank>(pa.BankId, ct);
                                if (bank != null) pa.Bank = bank;
                                else pa.BankId = 0;
                            }

                            // 2) Nav ile bağla/oluştur
                            if (pa.Bank == null)
                            {
                                await AttachLookupByFkOrNavOrEnsure<Bank>(
                                    pa,
                                    "BankId",
                                    "Bank",
                                    nav => new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                                    {
                                        ["Name"] = nav?.GetType().GetProperty("Name")?.GetValue(nav)?.ToString(),
                                        ["SwiftCode"] = nav?.GetType().GetProperty("SwiftCode")?.GetValue(nav)?.ToString(),
                                        ["Country"] = nav?.GetType().GetProperty("Country")?.GetValue(nav)?.ToString(),
                                        ["City"] = nav?.GetType().GetProperty("City")?.GetValue(nav)?.ToString(),
                                    },
                                    ct);
                            }

                            // 3) Hâlâ yoksa default banka
                            if (pa.Bank == null && pa.BankId == 0)
                            {
                                var defBank = await EnsureDefaultBankAsync(ct);
                                pa.Bank = defBank;
                            }

                            // NOT NULL alanları garanti et
                            NormalizePaymentAccount(pa, p.Currency);
                            if (pa.Bank != null) NormalizeBank(pa.Bank);

                            // Döngü kır
                            pa.Payments = null;
                        }

                        // Döngü kır
                        p.InvoicesPayments = null;
                    }
                }
            }

            // ---- Discounts (varsa ilk item’a bağla)
            if (inv.InvoicesDiscounts != null && inv.InvoicesDiscounts.Count > 0)
            {
                var firstItem = inv.InvoicesItems?.FirstOrDefault()?.Item;
                foreach (var d in inv.InvoicesDiscounts)
                {
                    if (d.CreatedAt == default) d.CreatedAt = now;
                    d.UpdatedAt = now;
                    if (firstItem != null) d.Item = firstItem;
                }
            }

            // ---- SGK / SP / Returns / Tourists zaman damgası
            foreach (var sg in inv.SgkRecords ?? Enumerable.Empty<Sgk>())
            { if (sg.CreatedAt == default) sg.CreatedAt = now; sg.UpdatedAt = now; }

            foreach (var sp in inv.ServicesProviders ?? Enumerable.Empty<ServicesProvider>())
            { if (sp.CreatedAt == default) sp.CreatedAt = now; sp.UpdatedAt = now; }

            foreach (var r in inv.Returns ?? Enumerable.Empty<Returns>())
            { if (r.CreatedAt == default) r.CreatedAt = now; r.UpdatedAt = now; }

            foreach (var t in inv.Tourists ?? Enumerable.Empty<Tourist>())
            { if (t.CreatedAt == default) t.CreatedAt = now; t.UpdatedAt = now; }

            // ---- Invoice temel alanlar
            if (string.IsNullOrWhiteSpace(inv.Currency)) inv.Currency = "TRY";
            if (inv.InvoiceDate == default) inv.InvoiceDate = now;
            if (inv.CreatedAt == default) inv.CreatedAt = now;
            inv.UpdatedAt = now;

            // İstemci total göndermediyse hesapla
            if (inv.Total <= 0 && inv.InvoicesItems?.Any() == true)
            {
                var gross = inv.InvoicesItems.Sum(x => x.Total);
                if (inv.InvoicesTaxes?.Any() == true) gross += inv.InvoicesTaxes.Sum(x => x.Amount);
                inv.Total = gross;
            }
        }
    }
}
