using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
    /// - BaseEntity audit alanları DateTimeOffset (UTC) olarak yönetilir.
    /// - UserId + iş anahtarı (Name/Code/TaxNo vb.) kombinasyonuna göre mükerrer kayıtları engeller.
    /// - Aynı anahtarla aktif kayıt varsa insert yapmaz, var olan kaydı kullanır.
    /// </summary>
    public class EntityGibManager<TEntity, TKey> : IEntityGibService<TEntity, TKey>
        where TEntity : class
    {
        private readonly GibAppDbContext _db;
        private readonly DbSet<TEntity> _set;

        private static readonly PropertyInfo IdProp = typeof(TEntity).GetProperty("Id")
            ?? throw new InvalidOperationException($"'{typeof(TEntity).Name}' üzerinde 'Id' alanı yok.");

        private static readonly PropertyInfo? IsActiveProp = typeof(TEntity).GetProperty("IsActive");

        // UserScopedUniqueness ile uyumlu iş anahtarı adayları
        private static readonly string[] UniqueKeyCandidates =
        {
            "Code", "Name", "Title", "TaxNo", "IsoCode", "Email", "Username", "Sku", "Barcode"
        };

        // Bazı entity’ler için tercih edilen iş anahtarları
        private static readonly Dictionary<string, string[]> PreferredUniqueKeyByEntity =
            new Dictionary<string, string[]>
            {
                { "Item",               new[] { "Code", "Name" } },
                { "CompanyInformation", new[] { "TaxNo" } },
                { "Currency",           new[] { "Code" } },
                { "Country",            new[] { "IsoCode", "Code" } },
                { "Brand",              new[] { "Name" } },
                { "Category",           new[] { "Name" } },
                { "Unit",               new[] { "ShortName", "Name" } },
                { "Warehouse",          new[] { "Name" } },
                { "UserRole",           new[] { "RoleId" } },
                { "Bank",               new[] { "Name" } },
                { "PaymentType",        new[] { "Name" } },
                { "PaymentAccount",     new[] { "Name" } },
                { "Customer",           new[] { "TaxNo", "Name" } },
                { "GibFirm",            new[] { "TaxNo" } },
                // İstersen buraya "Identifiers" -> "Uuid" da ekleyebilirsin;
                // şu an için Identifiers'ı yeni kayıt olarak ele alıyoruz.
            };

        // SQL decimal(18,2) için max limit
        private const decimal SqlDecimal18_2Max = 9999999999999999.99m;

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

            // 🔹 TEntity için navigation’ları dahil et
            query = ApplyIncludesForEntity(query);

            var predicate = BuildIdEqualsExpression(id);
            return await query.FirstOrDefaultAsync(predicate, ct);
        }

        public async Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
        {
            var query = _set.AsNoTracking().AsQueryable();
            query = ApplyIsActiveFilterIfExists(query);

            // 🔹 TEntity için navigation’ları dahil et
            query = ApplyIncludesForEntity(query);

            return await query.ToListAsync(ct);
        }




        /// <summary>
        /// Generic Add.
        /// - Eğer TEntity = Item ise, FixItemGraphAsync ile graph normalize edilir.
        /// - Eğer TEntity = Customer ise, FixCustomerGraphAsync ile graph normalize edilir.
        /// - Eğer TEntity = Invoice ise, FixInvoiceGraphAsync ile graph normalize edilir.
        /// - UserId + iş anahtarına göre duplicate kontrolü yapılır.
        /// </summary>
        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default)
        {
            var prevTimeout = _db.Database.GetCommandTimeout();
            _db.Database.SetCommandTimeout(TimeSpan.FromSeconds(120));

            var utcNow = DateTimeOffset.UtcNow;

            // ✅ retry varsa bile güvenli çalıştır
            var strategy = _db.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _db.Database.BeginTransactionAsync(ct);

                    try
                    {
                        // 0) Eğer Id > 0 geldiyse: önce DB’de böyle bir kayıt var mı bak
                        if (IdProp != null)
                        {
                            var idValue = IdProp.GetValue(entity);
                            if (idValue != null)
                            {
                                var idType = Nullable.GetUnderlyingType(IdProp.PropertyType) ?? IdProp.PropertyType;
                                if (idType == typeof(long) || idType == typeof(int) || idType == typeof(short))
                                {
                                    var numeric = Convert.ToInt64(idValue);
                                    if (numeric > 0)
                                    {
                                        var existingById = await _set.FindAsync(new object[] { idValue }, ct);
                                        if (existingById != null)
                                        {
                                            await tx.RollbackAsync(ct);
                                            return existingById;
                                        }
                                        ResetIdentityId(entity);
                                    }
                                }
                            }
                        }

                        // 1) Audit
                        TouchAudit(entity, utcNow);

                        // ✅ 2) GibFirm graph fix (Invoice'a dokunma, rowversion insert etme)
                        if (entity is GibFirm firm)
                        {
                            FixGibFirmGraphForAdd(firm, utcNow);
                        }

                        // 2-a) Item / 2-b) Customer / 2-c) Invoice aynı kalsın
                        if (entity is Item itemEntity)
                            await FixItemGraphAsync(itemEntity, utcNow, ct);

                        if (entity is Customer custEntity)
                            await FixCustomerGraphAsync(custEntity, utcNow, ct);

                        if (entity is Invoice inv)
                        {
                            await FixInvoiceGraphAsync(inv, utcNow, ct);
                            ValidateInvoiceDecimalRanges(inv);
                        }

                        // 3) Duplicate kontrol
                        var existingByKey = await TryGetExistingByUniqueKeyAsync(_db, entity, ct);
                        if (existingByKey != null)
                        {
                            await tx.RollbackAsync(ct);
                            return existingByKey;
                        }

                        // 4) Insert
                        await _set.AddAsync(entity, ct);
                        await _db.SaveChangesAsync(ct);

                        await tx.CommitAsync(ct);
                        return entity;
                    }
                    catch
                    {
                        await tx.RollbackAsync(ct);
                        _db.ChangeTracker.Clear(); // ✅ retry-safe + state temizliği
                        throw;
                    }
                });
            }
            catch (DbUpdateException due) when (IsUniqueViolation(due))
            {
                var existing = await TryGetExistingByUniqueKeyAsync(_db, entity, ct);
                if (existing != null)
                {
                    _db.ChangeTracker.Clear();
                    return existing;
                }
                throw new Exception(BuildUniqueViolationMessage<TEntity>(due), due);
            }
            finally
            {
                _db.Database.SetCommandTimeout(prevTimeout);
            }
        }


        public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken ct = default)
        {
            // UpdatedAt güncelle
            TouchAudit(entity, DateTimeOffset.UtcNow);

            _set.Update(entity);
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException due) when (IsUniqueViolation(due))
            {
                throw new Exception(BuildUniqueViolationMessage<TEntity>(due), due);
            }
            return entity;
        }

        public async Task<bool> DeleteAsync(TKey id, CancellationToken ct = default)
        {
            var entity = await _set.FirstOrDefaultAsync(BuildIdEqualsExpression(id), ct);
            if (entity == null) return false;

            var utcNow = DateTimeOffset.UtcNow;

            // BaseEntity ise soft delete
            if (entity is BaseEntity be)
            {
                be.IsActive = false;
                be.DeleteDate = utcNow;
                _set.Update(entity);
            }
            else if (IsActiveProp != null && IsActiveProp.PropertyType == typeof(bool))
            {
                // BaseEntity olmayan ama IsActive alanı olan bir tipse
                IsActiveProp.SetValue(entity, false);
                _set.Update(entity);
            }
            else
            {
                // Hiçbiri değilse hard delete
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
                idValue = Convert.ChangeType(
                    id,
                    Nullable.GetUnderlyingType(IdProp.PropertyType) ?? IdProp.PropertyType);
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

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            var msg = (ex.InnerException?.Message ?? ex.Message)?.ToLowerInvariant() ?? "";
            return msg.Contains("2601") || msg.Contains("2627") ||
                   msg.Contains("cannot insert duplicate key") ||
                   msg.Contains("with unique index") ||
                   msg.Contains("unique constraint");
        }

        private static string BuildUniqueViolationMessage<T>(DbUpdateException ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return $"Aynı veriden mükerrer kayıt eklenemez (tablo: {typeof(T).Name}). Detay: {msg}";
        }

        /// <summary>
        /// Herhangi bir entity için "Id" property’si integer tipindeyse ve > 0 ise, 0'a çeker.
        /// Identity kolon insert hatalarını engellemek için kullanılır.
        /// </summary>
        private static void ResetIdentityId(object? obj)
        {
            if (obj == null) return;

            var type = obj.GetType();
            var idProp = type.GetProperty("Id");
            if (idProp == null) return;

            var propType = Nullable.GetUnderlyingType(idProp.PropertyType) ?? idProp.PropertyType;
            var current = idProp.GetValue(obj);
            if (current == null) return;

            try
            {
                if (propType == typeof(long))
                {
                    long v = Convert.ToInt64(current);
                    if (v > 0) idProp.SetValue(obj, 0L);
                }
                else if (propType == typeof(int))
                {
                    int v = Convert.ToInt32(current);
                    if (v > 0) idProp.SetValue(obj, 0);
                }
                else if (propType == typeof(short))
                {
                    short v = Convert.ToInt16(current);
                    if (v > 0) idProp.SetValue(obj, (short)0);
                }
            }
            catch
            {
                // Tip uyumsuzluğu vs. olursa sessiz geç.
            }
        }

        // ======================================================
        // Audit helper (CreatedAt / UpdatedAt / IsActive)
        // ======================================================

        private static void TouchAudit(object entity, DateTimeOffset utcNow)
        {
            if (entity is not BaseEntity be) return;

            if (be.CreatedAt == default)
                be.CreatedAt = utcNow;

            be.UpdatedAt = utcNow;

            if (!be.IsActive)
                be.IsActive = true;
        }

        /// <summary>
        /// Parent - child ilişkisinde UserId propagation.
        /// Child.UserId = 0 ise parent.UserId değeriyle doldurur.
        /// </summary>
        private static void PropagateUserId(BaseEntity parent, BaseEntity child)
        {
            if (parent == null || child == null) return;

            if (child.UserId == 0 && parent.UserId != 0)
            {
                child.UserId = parent.UserId;
            }
        }

        // ======================================================
        // Duplicate Engelleyici (UserId + İş Anahtarı)
        // ======================================================

        private static async Task<TRef?> TryGetExistingByUniqueKeyAsync<TRef>(
            GibAppDbContext db,
            TRef entity,
            CancellationToken ct)
            where TRef : class
        {
            if (entity == null) return null;

            var type = typeof(TRef);
            var set = db.Set<TRef>();

            // 1) İş anahtarı property’sini bul
            PropertyInfo? keyProp = null;

            if (PreferredUniqueKeyByEntity.TryGetValue(type.Name, out var preferredList))
            {
                foreach (var name in preferredList)
                {
                    keyProp = type.GetProperty(name);
                    if (keyProp != null) break;
                }
            }

            if (keyProp == null)
            {
                foreach (var cand in UniqueKeyCandidates)
                {
                    keyProp = type.GetProperty(cand);
                    if (keyProp != null) break;
                }
            }

            if (keyProp == null)
                return null;

            var keyValue = keyProp.GetValue(entity);
            if (keyValue == null)
                return null;

            if (keyValue is string s && string.IsNullOrWhiteSpace(s))
                return null;

            var param = Expression.Parameter(type, "e");

            Expression body = Expression.Equal(
                Expression.Property(param, keyProp),
                Expression.Constant(keyValue, keyProp.PropertyType)
            );

            // 2) UserId varsa onu da filtreye ekle
            var userIdProp = type.GetProperty("UserId");
            if (userIdProp != null)
            {
                var userIdValue = userIdProp.GetValue(entity);
                if (userIdValue != null)
                {
                    var targetUserType = Nullable.GetUnderlyingType(userIdProp.PropertyType) ?? userIdProp.PropertyType;
                    var convertedUserId = Convert.ChangeType(userIdValue, targetUserType);

                    var userExpr = Expression.Equal(
                        Expression.Property(param, userIdProp),
                        Expression.Constant(convertedUserId, userIdProp.PropertyType)
                    );

                    body = Expression.AndAlso(userExpr, body);
                }
            }

            // 3) IsActive == true (varsa)
            var isActiveProp = type.GetProperty("IsActive");
            if (isActiveProp != null && isActiveProp.PropertyType == typeof(bool))
            {
                var activeExpr = Expression.Equal(
                    Expression.Property(param, isActiveProp),
                    Expression.Constant(true)
                );
                body = Expression.AndAlso(activeExpr, body);
            }

            var lambda = Expression.Lambda<Func<TRef, bool>>(body, param);

            return await set.AsNoTracking().FirstOrDefaultAsync(lambda, ct);
        }

        private static Task<TEntity?> TryGetExistingByUniqueKeyAsync(
            GibAppDbContext db,
            TEntity entity,
            CancellationToken ct)
            => TryGetExistingByUniqueKeyAsync<TEntity>(db, entity, ct);

        // ======================================================
        // Normalizers (zorunlu alanlar)
        // ======================================================

        private static void NormalizeCustomer(Customer c, DateTimeOffset utcNow)
        {
            c.Name = string.IsNullOrWhiteSpace(c.Name) ? "GENEL MÜŞTERİ" : c.Name.Trim();
            c.Surname = string.IsNullOrWhiteSpace(c.Surname) ? "." : c.Surname.Trim();
            c.Phone = string.IsNullOrWhiteSpace(c.Phone) ? "-" : c.Phone.Trim();
            c.Email = (c.Email ?? string.Empty).Trim();
            c.TaxNo = (c.TaxNo ?? string.Empty).Trim();
            c.TaxOffice = (c.TaxOffice ?? string.Empty).Trim();

            TouchAudit(c, utcNow);
        }

        private static void NormalizeBrand(Brand b, DateTimeOffset utcNow)
        {
            b.Name = string.IsNullOrWhiteSpace(b.Name) ? "GENEL" : b.Name.Trim();
            b.Country = string.IsNullOrWhiteSpace(b.Country) ? "TR" : b.Country.Trim();

            TouchAudit(b, utcNow);
        }

        private static void NormalizeUnit(Unit u, DateTimeOffset utcNow)
        {
            u.ShortName = string.IsNullOrWhiteSpace(u.ShortName) ? "C62" : u.ShortName.Trim();
            u.Name = string.IsNullOrWhiteSpace(u.Name)
                ? (u.ShortName == "C62" ? "ADET" : u.ShortName)
                : u.Name.Trim();

            TouchAudit(u, utcNow);
        }

        private static void NormalizeCategory(Category c, DateTimeOffset utcNow)
        {
            c.Name = string.IsNullOrWhiteSpace(c.Name) ? "GENEL KATEGORI" : c.Name.Trim();
            c.Desc = (c.Desc ?? string.Empty).Trim();

            TouchAudit(c, utcNow);
        }

        private static void NormalizePaymentType(PaymentType pt, DateTimeOffset utcNow)
        {
            pt.Name = string.IsNullOrWhiteSpace(pt.Name) ? "NAKIT" : pt.Name.Trim();
            if (string.IsNullOrWhiteSpace(pt.Desc))
                pt.Desc = pt.Name;

            TouchAudit(pt, utcNow);
        }

        private static void NormalizeBank(Bank b, DateTimeOffset utcNow)
        {
            b.Name = string.IsNullOrWhiteSpace(b.Name) ? "Ziraat Bankası" : b.Name.Trim();
            b.SwiftCode = string.IsNullOrWhiteSpace(b.SwiftCode) ? "TCZBTR2A" : b.SwiftCode.Trim();
            b.Country = string.IsNullOrWhiteSpace(b.Country) ? "TR" : b.Country.Trim();
            b.City = string.IsNullOrWhiteSpace(b.City) ? "Ankara" : b.City.Trim();

            TouchAudit(b, utcNow);
        }

        private static void NormalizePaymentAccount(PaymentAccount pa, string defaultCurrency, DateTimeOffset utcNow)
        {
            pa.Name = string.IsNullOrWhiteSpace(pa.Name) ? "KASA" : pa.Name.Trim();

            if (string.IsNullOrWhiteSpace(pa.Desc))
                pa.Desc = "Varsayılan Kasa";

            if (string.IsNullOrWhiteSpace(pa.AccountNo))
                pa.AccountNo = "0001";

            if (string.IsNullOrWhiteSpace(pa.Iban))
                pa.Iban = "TR000000000000000000000000";

            if (string.IsNullOrWhiteSpace(pa.Currency))
                pa.Currency = string.IsNullOrWhiteSpace(defaultCurrency) ? "TRY" : defaultCurrency;

            TouchAudit(pa, utcNow);
        }

        private static void NormalizeItem(Item it, string defaultCurrency, DateTimeOffset utcNow)
        {
            if (string.IsNullOrWhiteSpace(it.Name))
                it.Name = "GENEL ÜRÜN";

            if (string.IsNullOrWhiteSpace(it.Code))
                it.Code = it.Name;

            if (string.IsNullOrWhiteSpace(it.Currency))
                it.Currency = string.IsNullOrWhiteSpace(defaultCurrency) ? "TRY" : defaultCurrency;

            TouchAudit(it, utcNow);
        }

        private static void NormalizeSgk(Sgk sg, Invoice inv, DateTimeOffset utcNow)
        {
            sg.Type = string.IsNullOrWhiteSpace(sg.Type) ? "GENEL" : sg.Type.Trim();
            sg.Code = (sg.Code ?? string.Empty).Trim();
            sg.Name = (sg.Name ?? string.Empty).Trim();
            sg.No = (sg.No ?? string.Empty).Trim();

            if (sg.StartDate == default)
                sg.StartDate = inv.InvoiceDate != default ? inv.InvoiceDate : utcNow.UtcDateTime;

            if (sg.EndDate == default)
                sg.EndDate = sg.StartDate;

            TouchAudit(sg, utcNow);
        }

        private static void NormalizeTourist(Tourist t, DateTimeOffset utcNow)
        {
            t.Name = string.IsNullOrWhiteSpace(t.Name) ? "MISAFIR" : t.Name.Trim();
            t.Surname = string.IsNullOrWhiteSpace(t.Surname) ? "." : t.Surname.Trim();
            t.PassportNo = string.IsNullOrWhiteSpace(t.PassportNo) ? "-" : t.PassportNo.Trim();
            t.Country = string.IsNullOrWhiteSpace(t.Country) ? "TR" : t.Country.Trim();
            t.City = (t.City ?? string.Empty).Trim();
            t.District = (t.District ?? string.Empty).Trim();
            t.AccountNo = (t.AccountNo ?? string.Empty).Trim();
            t.Bank = (t.Bank ?? string.Empty).Trim();
            t.Currency = string.IsNullOrWhiteSpace(t.Currency) ? "TRY" : t.Currency.Trim();
            t.Note = (t.Note ?? string.Empty).Trim();

            if (t.PassportDate == default)
                t.PassportDate = utcNow.UtcDateTime;

            TouchAudit(t, utcNow);
        }

        private static void NormalizeReturns(Returns r, Invoice inv, DateTimeOffset utcNow)
        {
            r.Number = string.IsNullOrWhiteSpace(r.Number) ? "-" : r.Number.Trim();

            if (r.Date == default)
                r.Date = inv.InvoiceDate != default ? inv.InvoiceDate : utcNow.UtcDateTime;

            TouchAudit(r, utcNow);
        }

        private static void NormalizeServicesProvider(ServicesProvider sp, DateTimeOffset utcNow)
        {
            sp.No = (sp.No ?? string.Empty).Trim();
            sp.SystemUser = (sp.SystemUser ?? string.Empty).Trim();

            TouchAudit(sp, utcNow);
        }

        private static void NormalizePayment(Payment p, Invoice inv, DateTimeOffset utcNow)
        {
            if (string.IsNullOrWhiteSpace(p.Currency))
                p.Currency = string.IsNullOrWhiteSpace(inv.Currency) ? "TRY" : inv.Currency;

            if (p.Date == default)
                p.Date = utcNow.UtcDateTime;

            p.Note = (p.Note ?? string.Empty).Trim();

            TouchAudit(p, utcNow);
        }

        private static void NormalizeInvoice(Invoice inv, DateTimeOffset utcNow)
        {
            if (string.IsNullOrWhiteSpace(inv.Currency))
                inv.Currency = "TRY";

            if (inv.InvoiceDate == default)
                inv.InvoiceDate = utcNow.UtcDateTime;

            if (string.IsNullOrWhiteSpace(inv.InvoiceNo))
                inv.InvoiceNo = $"INV-{utcNow:yyyyMMddHHmmss}";

            TouchAudit(inv, utcNow);
        }

        private static void NormalizeGroup(Group g, DateTimeOffset utcNow)
        {
            g.Name = string.IsNullOrWhiteSpace(g.Name) ? "GENEL GRUP" : g.Name.Trim();
            g.Desc = (g.Desc ?? string.Empty).Trim();

            TouchAudit(g, utcNow);
        }

        private static void NormalizeAddress(Address a, DateTimeOffset utcNow)
        {
            a.Country = string.IsNullOrWhiteSpace(a.Country) ? "TÜRKİYE" : a.Country.Trim();
            a.City = (a.City ?? string.Empty).Trim();
            a.District = (a.District ?? string.Empty).Trim();
            a.Street = (a.Street ?? string.Empty).Trim();
            a.PostCode = (a.PostCode ?? string.Empty).Trim();

            TouchAudit(a, utcNow);
        }

        // ======================================================
        // Attach / Create Helpers
        // ======================================================

        private async Task AttachOrCreateBrandAsync(Item item, CancellationToken ct, DateTimeOffset utcNow)
        {
            var brand = item.Brand;
            if (brand == null) return;

            PropagateUserId(item, brand);

            if (brand.Id > 0)
            {
                var existing = await _db.Set<Brand>().FindAsync(new object[] { brand.Id }, ct);
                if (existing != null)
                {
                    _db.Attach(existing);
                    _db.Entry(existing).State = EntityState.Unchanged;
                    item.Brand = existing;
                    item.BrandId = existing.Id;
                    return;
                }
            }

            var existingByKey = await TryGetExistingByUniqueKeyAsync(_db, brand, ct);
            if (existingByKey != null)
            {
                _db.Attach(existingByKey);
                _db.Entry(existingByKey).State = EntityState.Unchanged;
                item.Brand = existingByKey;
                item.BrandId = existingByKey.Id;
                return;
            }

            ResetIdentityId(brand);
            NormalizeBrand(brand, utcNow);
            brand.Items = null;
        }

        private async Task AttachOrCreateUnitAsync(Item item, CancellationToken ct, DateTimeOffset utcNow)
        {
            var unit = item.Unit;
            if (unit == null) return;

            PropagateUserId(item, unit);

            if (unit.Id > 0)
            {
                var existing = await _db.Set<Unit>().FindAsync(new object[] { unit.Id }, ct);
                if (existing != null)
                {
                    _db.Attach(existing);
                    _db.Entry(existing).State = EntityState.Unchanged;
                    item.Unit = existing;
                    item.UnitId = existing.Id;
                    return;
                }
            }

            var existingByKey = await TryGetExistingByUniqueKeyAsync(_db, unit, ct);
            if (existingByKey != null)
            {
                _db.Attach(existingByKey);
                _db.Entry(existingByKey).State = EntityState.Unchanged;
                item.Unit = existingByKey;
                item.UnitId = existingByKey.Id;
                return;
            }

            ResetIdentityId(unit);
            NormalizeUnit(unit, utcNow);
            unit.Items = null;
        }

        private async Task AttachOrCreateCategoryAsync(ItemsCategory ic, CancellationToken ct, DateTimeOffset utcNow)
        {
            var cat = ic.Category;
            if (cat == null) return;

            PropagateUserId(ic, cat);

            if (cat.Id > 0)
            {
                var existing = await _db.Set<Category>().FindAsync(new object[] { cat.Id }, ct);
                if (existing != null)
                {
                    _db.Attach(existing);
                    _db.Entry(existing).State = EntityState.Unchanged;
                    ic.Category = existing;
                    ic.CategoryId = existing.Id;
                    return;
                }
            }

            var existingByKey = await TryGetExistingByUniqueKeyAsync(_db, cat, ct);
            if (existingByKey != null)
            {
                _db.Attach(existingByKey);
                _db.Entry(existingByKey).State = EntityState.Unchanged;
                ic.Category = existingByKey;
                ic.CategoryId = existingByKey.Id;
                return;
            }

            ResetIdentityId(cat);
            NormalizeCategory(cat, utcNow);
            cat.ItemsCategories = null;
        }

        private async Task AttachOrCreatePaymentTypeAsync(Payment p, CancellationToken ct, DateTimeOffset utcNow)
        {
            var pt = p.PaymentType;
            if (pt == null) return;

            PropagateUserId(p, pt);

            if (pt.Id > 0)
            {
                var existing = await _db.Set<PaymentType>().FindAsync(new object[] { pt.Id }, ct);
                if (existing != null)
                {
                    _db.Attach(existing);
                    _db.Entry(existing).State = EntityState.Unchanged;
                    p.PaymentType = existing;
                    p.PaymentTypeId = existing.Id;
                    return;
                }
            }

            var existingByKey = await TryGetExistingByUniqueKeyAsync(_db, pt, ct);
            if (existingByKey != null)
            {
                _db.Attach(existingByKey);
                _db.Entry(existingByKey).State = EntityState.Unchanged;
                p.PaymentType = existingByKey;
                p.PaymentTypeId = existingByKey.Id;
                return;
            }

            ResetIdentityId(pt);
            NormalizePaymentType(pt, utcNow);
            pt.Payments = null;
        }

        private async Task AttachOrCreateBankAsync(PaymentAccount pa, CancellationToken ct, DateTimeOffset utcNow)
        {
            var bank = pa.Bank;
            if (bank == null) return;

            PropagateUserId(pa, bank);

            if (bank.Id > 0)
            {
                var existing = await _db.Set<Bank>().FindAsync(new object[] { bank.Id }, ct);
                if (existing != null)
                {
                    _db.Attach(existing);
                    _db.Entry(existing).State = EntityState.Unchanged;
                    pa.Bank = existing;
                    pa.BankId = existing.Id;
                    return;
                }
            }

            var existingByKey = await TryGetExistingByUniqueKeyAsync(_db, bank, ct);
            if (existingByKey != null)
            {
                _db.Attach(existingByKey);
                _db.Entry(existingByKey).State = EntityState.Unchanged;
                pa.Bank = existingByKey;
                pa.BankId = existingByKey.Id;
                return;
            }

            ResetIdentityId(bank);
            NormalizeBank(bank, utcNow);
            bank.PaymentAccounts = null;
        }

        private async Task AttachOrCreatePaymentAccountAsync(Payment p, CancellationToken ct, DateTimeOffset utcNow)
        {
            var pa = p.PaymentAccount;
            if (pa == null) return;

            PropagateUserId(p, pa);

            if (pa.Id > 0)
            {
                var existing = await _db.Set<PaymentAccount>().FindAsync(new object[] { pa.Id }, ct);
                if (existing != null)
                {
                    _db.Attach(existing);
                    _db.Entry(existing).State = EntityState.Unchanged;
                    p.PaymentAccount = existing;
                    p.PaymentAccountId = existing.Id;
                    return;
                }
            }

            var existingByKey = await TryGetExistingByUniqueKeyAsync(_db, pa, ct);
            if (existingByKey != null)
            {
                _db.Attach(existingByKey);
                _db.Entry(existingByKey).State = EntityState.Unchanged;
                p.PaymentAccount = existingByKey;
                p.PaymentAccountId = existingByKey.Id;
                return;
            }

            ResetIdentityId(pa);
            NormalizePaymentAccount(pa, p.Currency, utcNow);
            await AttachOrCreateBankAsync(pa, ct, utcNow);
            pa.Payments = null;
        }

        private async Task AttachOrCreateGroupAsync(CustomersGroup cg, CancellationToken ct, DateTimeOffset utcNow)
        {
            var grp = cg.Group;
            if (grp == null) return;

            PropagateUserId(cg, grp);

            if (grp.Id > 0)
            {
                var existing = await _db.Set<Group>().FindAsync(new object[] { grp.Id }, ct);
                if (existing != null)
                {
                    _db.Attach(existing);
                    _db.Entry(existing).State = EntityState.Unchanged;
                    cg.Group = existing;
                    cg.GroupId = existing.Id;
                    return;
                }
            }

            var existingByKey = await TryGetExistingByUniqueKeyAsync(_db, grp, ct);
            if (existingByKey != null)
            {
                _db.Attach(existingByKey);
                _db.Entry(existingByKey).State = EntityState.Unchanged;
                cg.Group = existingByKey;
                cg.GroupId = existingByKey.Id;
                return;
            }

            ResetIdentityId(grp);
            NormalizeGroup(grp, utcNow);
            grp.CustomersGroups = null;
        }

        // ======================================================
        // Decimal range validator
        // ======================================================

        private static void EnsureDecimalInSqlRange(decimal value, string context, string propertyName)
        {
            if (value > SqlDecimal18_2Max || value < -SqlDecimal18_2Max)
            {
                throw new ArgumentOutOfRangeException(
                    $"{context}.{propertyName}",
                    value,
                    $"Değer SQL decimal(18,2) aralığının dışında. Context={context}, Property={propertyName}, Value={value}");
            }
        }

        private static void ValidateDecimalProperties(object? obj, string context)
        {
            if (obj == null) return;

            var type = obj.GetType();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(decimal))
                {
                    var raw = prop.GetValue(obj);
                    if (raw != null)
                    {
                        var val = (decimal)raw;
                        EnsureDecimalInSqlRange(val, $"{context}.{type.Name}", prop.Name);
                    }
                }
                else if (prop.PropertyType == typeof(decimal?))
                {
                    var raw = prop.GetValue(obj);
                    if (raw is decimal val)
                    {
                        EnsureDecimalInSqlRange(val, $"{context}.{type.Name}", prop.Name);
                    }
                }
            }
        }

        private static void ValidateInvoiceDecimalRanges(Invoice inv)
        {
            // Root invoice
            ValidateDecimalProperties(inv, "Invoice");

            // Items
            if (inv.InvoicesItems != null)
            {
                foreach (var li in inv.InvoicesItems)
                {
                    ValidateDecimalProperties(li, "Invoice.InvoicesItems");

                    if (li.Item != null)
                    {
                        ValidateDecimalProperties(li.Item, "Invoice.InvoicesItems.Item");

                        // Item.ItemsDiscounts (Amount/Rate)
                        if (li.Item.ItemsDiscounts != null)
                        {
                            foreach (var idisc in li.Item.ItemsDiscounts)
                            {
                                ValidateDecimalProperties(idisc, "Invoice.InvoicesItems.Item.ItemsDiscounts");
                            }
                        }
                    }
                }
            }

            // Taxes
            if (inv.InvoicesTaxes != null)
            {
                foreach (var tx in inv.InvoicesTaxes)
                    ValidateDecimalProperties(tx, "Invoice.InvoicesTaxes");
            }

            // Discounts
            if (inv.InvoicesDiscounts != null)
            {
                foreach (var d in inv.InvoicesDiscounts)
                    ValidateDecimalProperties(d, "Invoice.InvoicesDiscounts");
            }

            // Payments + Payment entities
            if (inv.InvoicesPayments != null)
            {
                foreach (var link in inv.InvoicesPayments)
                {
                    ValidateDecimalProperties(link, "Invoice.InvoicesPayments");
                    if (link.Payment != null)
                        ValidateDecimalProperties(link.Payment, "Invoice.InvoicesPayments.Payment");
                }
            }
        }

        // ======================================================
        // Invoice Graph Fixer
        // ======================================================
        private async Task FixInvoiceGraphAsync(Invoice inv, DateTimeOffset utcNow, CancellationToken ct)
        {
            // Root
            TouchAudit(inv, utcNow);

            // ---------------------------
            // 1) CUSTOMER
            // ---------------------------
            Customer? cust = inv.Customer;
            long customerId = inv.CustomerId;

            if (cust != null)
            {
                PropagateUserId(inv, cust);
            }

            if (cust != null && cust.Id > 0 && customerId == 0)
                customerId = cust.Id;

            // Önce Id ile kontrol
            if (customerId > 0)
            {
                var existingById = await _db.Set<Customer>().FindAsync(new object[] { customerId }, ct);
                if (existingById != null)
                {
                    _db.Attach(existingById);
                    _db.Entry(existingById).State = EntityState.Unchanged;
                    inv.Customer = existingById;
                    inv.CustomerId = existingById.Id;
                }
                else
                {
                    inv.CustomerId = 0;
                }
            }

            // Id bulunamadıysa; key'e göre veya yeni müşteri
            if (inv.CustomerId == 0)
            {
                if (cust == null)
                {
                    cust = new Customer { Name = "GENEL MÜŞTERİ" };
                    PropagateUserId(inv, cust);
                    inv.Customer = cust;
                }

                var existingByKey = await TryGetExistingByUniqueKeyAsync(_db, cust, ct);
                if (existingByKey != null)
                {
                    _db.Attach(existingByKey);
                    _db.Entry(existingByKey).State = EntityState.Unchanged;
                    inv.Customer = existingByKey;
                    inv.CustomerId = existingByKey.Id;
                }
                else
                {
                    // Yeni müşteri
                    ResetIdentityId(cust);
                    NormalizeCustomer(cust, utcNow);
                    inv.CustomerId = 0;

                    // Customer master verisini sade tutmak için;
                    // İstersen bunları da ayrı senaryoda yönetebilirsin
                    cust.Invoices = null;
                }
            }

            // ---------------------------
            // 2) Koleksiyon init
            // ---------------------------
            inv.InvoicesItems ??= new List<InvoicesItem>();
            inv.InvoicesTaxes ??= new List<InvoicesTax>();
            inv.InvoicesDiscounts ??= new List<InvoicesDiscount>();
            inv.Tourists ??= new List<Tourist>();
            inv.SgkRecords ??= new List<Sgk>();
            inv.ServicesProviders ??= new List<ServicesProvider>();
            inv.Returns ??= new List<Returns>();
            inv.InvoicesPayments ??= new List<InvoicesPayment>();
            inv.GibInvoiceOperationLogs ??= new List<GibInvoiceOperationLog>();
            inv.GibUserCreditTransactions ??= new List<GibUserCreditTransaction>();

            // ---------------------------
            // 3) ITEMS (InvoicesItem -> Item + alt nav'lar)
            // ---------------------------
            foreach (var li in inv.InvoicesItems)
            {
                PropagateUserId(inv, li);
                TouchAudit(li, utcNow);
                li.Invoice = null;

                var it = li.Item;
                if (it != null)
                {
                    PropagateUserId(inv, it);

                    // Var olan Item Id'si varsa önce ona bak
                    if (it.Id > 0)
                    {
                        var existingItem = await _db.Set<Item>().FindAsync(new object[] { it.Id }, ct);
                        if (existingItem != null)
                        {
                            _db.Attach(existingItem);
                            _db.Entry(existingItem).State = EntityState.Unchanged;
                            li.Item = existingItem;
                            li.ItemId = existingItem.Id;
                            it = existingItem;
                        }
                        else
                        {
                            it.Id = 0;
                        }
                    }

                    // İş anahtarına göre Item
                    if (it.Id == 0)
                    {
                        var existingByKey = await TryGetExistingByUniqueKeyAsync(_db, it, ct);
                        if (existingByKey != null)
                        {
                            _db.Attach(existingByKey);
                            _db.Entry(existingByKey).State = EntityState.Unchanged;
                            li.Item = existingByKey;
                            li.ItemId = existingByKey.Id;
                            it = existingByKey;
                        }
                    }

                    // Yeni Item
                    if (it.Id == 0)
                    {
                        ResetIdentityId(it);
                        NormalizeItem(it, inv.Currency, utcNow);

                        // BRAND
                        if (it.Brand != null)
                            await AttachOrCreateBrandAsync(it, ct, utcNow);
                        else if (it.BrandId > 0)
                        {
                            var existingBrand = await _db.Set<Brand>().FindAsync(new object[] { it.BrandId }, ct);
                            if (existingBrand != null)
                            {
                                _db.Attach(existingBrand);
                                _db.Entry(existingBrand).State = EntityState.Unchanged;
                                it.Brand = existingBrand;
                            }
                        }

                        // UNIT
                        if (it.Unit != null)
                            await AttachOrCreateUnitAsync(it, ct, utcNow);
                        else if (it.UnitId > 0)
                        {
                            var existingUnit = await _db.Set<Unit>().FindAsync(new object[] { it.UnitId }, ct);
                            if (existingUnit != null)
                            {
                                _db.Attach(existingUnit);
                                _db.Entry(existingUnit).State = EntityState.Unchanged;
                                it.Unit = existingUnit;
                            }
                        }

                        // ItemsCategory -> Category
                        if (it.ItemsCategories != null)
                        {
                            foreach (var ic in it.ItemsCategories)
                            {
                                PropagateUserId(it, ic);
                                TouchAudit(ic, utcNow);

                                // Item tarafındaki navigation döngüsünü kırmak istersen:
                                // EF, Item.ItemsCategories üzerinden zaten ilişki kuracak
                                ic.Item = null;

                                if (ic.Category != null)
                                {
                                    await AttachOrCreateCategoryAsync(ic, ct, utcNow);
                                }
                                else if (ic.CategoryId > 0)
                                {
                                    var existingCat = await _db.Set<Category>().FindAsync(new object[] { ic.CategoryId }, ct);
                                    if (existingCat != null)
                                    {
                                        _db.Attach(existingCat);
                                        _db.Entry(existingCat).State = EntityState.Unchanged;
                                        ic.Category = existingCat;
                                    }
                                }
                            }
                        }

                        // ItemsDiscounts (Item bazlı, InvoiceId içeren discount kayıtları)
                        if (it.ItemsDiscounts != null)
                        {
                            foreach (var idisc in it.ItemsDiscounts)
                            {
                                PropagateUserId(inv, idisc);
                                TouchAudit(idisc, utcNow);

                                // Bu discount'un hangi invoice'a ait olduğu:
                                if (idisc.Invoice == null)
                                    idisc.Invoice = inv;     // navigation
                                if (idisc.InvoiceId == 0)
                                    idisc.InvoiceId = inv.Id; // yeni kayıt = 0, EF sonrası günceller

                                // Burada deduplikasyon yapmıyoruz; her invoice için ayrı discount kaydı
                                ResetIdentityId(idisc);
                            }
                        }

                        // Identifiers (Item kimlikleri – GTIN, barkod vs.)
                        if (it.Identifiers != null)
                        {
                            foreach (var ident in it.Identifiers)
                            {
                                PropagateUserId(it, ident);
                                TouchAudit(ident, utcNow);

                                // Item nav'ını kırabilirsin; Item.Identifiers koleksiyonu üzerinden ilişki zaten kurulacak
                                ident.Item = null;

                                ResetIdentityId(ident);
                            }
                        }
                    }
                }
                else if (li.ItemId > 0)
                {
                    var existingItem = await _db.Set<Item>().FindAsync(new object[] { li.ItemId }, ct);
                    if (existingItem != null)
                    {
                        _db.Attach(existingItem);
                        _db.Entry(existingItem).State = EntityState.Unchanged;
                        li.Item = existingItem;
                    }
                }

                // Fiyat/Toplam normalize
                if (li.Price <= 0 && li.Item != null)
                    li.Price = li.Item.Price;

                if (li.Total <= 0)
                    li.Total = li.Quantity * li.Price;
            }

            // ---------------------------
            // 4) TAXES (InvoicesTax)
            // ---------------------------
            if (inv.InvoicesTaxes != null && inv.InvoicesTaxes.Count > 0)
            {
                var normalizedTaxes = new List<InvoicesTax>();

                foreach (var tx in inv.InvoicesTaxes)
                {
                    PropagateUserId(inv, tx);
                    TouchAudit(tx, utcNow);
                    tx.Invoice = null;

                    InvoicesTax? existing = null;

                    // a) Id ile
                    if (tx.Id > 0)
                    {
                        existing = await _db.Set<InvoicesTax>().FindAsync(new object[] { tx.Id }, ct);
                    }

                    // b) (UserId + Name) ile
                    if (existing == null)
                    {
                        existing = await TryGetExistingByUniqueKeyAsync(_db, tx, ct);
                    }

                    if (existing != null)
                    {
                        _db.Attach(existing);
                        _db.Entry(existing).State = EntityState.Unchanged;
                        normalizedTaxes.Add(existing);
                    }
                    else
                    {
                        ResetIdentityId(tx);
                        normalizedTaxes.Add(tx);
                    }
                }

                inv.InvoicesTaxes = normalizedTaxes;
            }

            // ---------------------------
            // 5) DISCOUNTS (InvoicesDiscount)
            // ---------------------------
            if (inv.InvoicesDiscounts != null && inv.InvoicesDiscounts.Count > 0)
            {
                var firstItem = inv.InvoicesItems.FirstOrDefault()?.Item;
                var normalizedDiscounts = new List<InvoicesDiscount>();

                foreach (var d in inv.InvoicesDiscounts)
                {
                    PropagateUserId(inv, d);
                    TouchAudit(d, utcNow);

                    if (firstItem != null && d.Item == null && d.ItemId == 0)
                        d.Item = firstItem;

                    InvoicesDiscount? existing = null;

                    if (d.Id > 0)
                    {
                        existing = await _db.Set<InvoicesDiscount>().FindAsync(new object[] { d.Id }, ct);
                    }

                    if (existing == null)
                    {
                        existing = await TryGetExistingByUniqueKeyAsync(_db, d, ct);
                    }

                    if (existing != null)
                    {
                        _db.Attach(existing);
                        _db.Entry(existing).State = EntityState.Unchanged;
                        normalizedDiscounts.Add(existing);
                    }
                    else
                    {
                        ResetIdentityId(d);
                        normalizedDiscounts.Add(d);
                    }
                }

                inv.InvoicesDiscounts = normalizedDiscounts;
            }

            // ---------------------------
            // 6) SGK / SP / RETURNS / TOURISTS
            // ---------------------------
            foreach (var sg in inv.SgkRecords)
            {
                PropagateUserId(inv, sg);
                NormalizeSgk(sg, inv, utcNow);
                sg.Invoice = null;
            }

            foreach (var sp in inv.ServicesProviders)
            {
                PropagateUserId(inv, sp);
                NormalizeServicesProvider(sp, utcNow);
                sp.Invoice = null;
            }

            foreach (var r in inv.Returns)
            {
                PropagateUserId(inv, r);
                NormalizeReturns(r, inv, utcNow);
                r.Invoice = null;
            }

            foreach (var t in inv.Tourists)
            {
                PropagateUserId(inv, t);
                NormalizeTourist(t, utcNow);
                t.Invoice = null;
            }

            // ---------------------------
            // 7) PAYMENTS (InvoicesPayment -> Payment -> PaymentType/PaymentAccount/Bank)
            // ---------------------------
            foreach (var link in inv.InvoicesPayments)
            {
                PropagateUserId(inv, link);
                TouchAudit(link, utcNow);
                link.Invoice = null;

                var p = link.Payment;
                if (p == null) continue;

                PropagateUserId(inv, p);

                if (p.Id > 0)
                {
                    var existingPayment = await _db.Set<Payment>().FindAsync(new object[] { p.Id }, ct);
                    if (existingPayment != null)
                    {
                        _db.Attach(existingPayment);
                        _db.Entry(existingPayment).State = EntityState.Unchanged;
                        link.Payment = existingPayment;
                        link.PaymentId = existingPayment.Id;
                        continue;
                    }

                    p.Id = 0;
                }

                NormalizePayment(p, inv, utcNow);

                // PaymentType
                if (p.PaymentType != null)
                    await AttachOrCreatePaymentTypeAsync(p, ct, utcNow);
                else if (p.PaymentTypeId > 0)
                {
                    var existingPt = await _db.Set<PaymentType>().FindAsync(new object[] { p.PaymentTypeId }, ct);
                    if (existingPt != null)
                    {
                        _db.Attach(existingPt);
                        _db.Entry(existingPt).State = EntityState.Unchanged;
                        p.PaymentType = existingPt;
                    }
                }

                // PaymentAccount + Bank
                if (p.PaymentAccount != null)
                    await AttachOrCreatePaymentAccountAsync(p, ct, utcNow);
                else if (p.PaymentAccountId > 0)
                {
                    var existingPa = await _db.Set<PaymentAccount>().FindAsync(new object[] { p.PaymentAccountId }, ct);
                    if (existingPa != null)
                    {
                        _db.Attach(existingPa);
                        _db.Entry(existingPa).State = EntityState.Unchanged;
                        p.PaymentAccount = existingPa;
                    }
                }

                p.InvoicesPayments = null;
            }

            // ---------------------------
            // 8) GİB LOG / CREDIT
            // ---------------------------

            // GibInvoiceOperationLog: BaseEntity değil, kendi CreatedAt ve UserId alanlarını yönetiyoruz.
            if (inv.GibInvoiceOperationLogs != null)
            {
                foreach (var log in inv.GibInvoiceOperationLogs)
                {
                    ResetIdentityId(log); // int Id > 0 ise sıfırlar

                    log.Invoice = inv;
                    if (log.CreatedAt == default)
                        log.CreatedAt = DateTime.UtcNow;

                    if (!log.UserId.HasValue || log.UserId <= 0)
                        log.UserId = inv.UserId;

                    log.User = null;
                }
            }

            // GibUserCreditTransaction: BaseEntity; UserId vs. propagate ediliyor
            if (inv.GibUserCreditTransactions != null)
            {
                foreach (var tr in inv.GibUserCreditTransactions)
                {
                    PropagateUserId(inv, tr);
                    TouchAudit(tr, utcNow);

                    if (!tr.InvoiceId.HasValue || tr.InvoiceId == 0)
                        tr.InvoiceId = inv.Id;   // yeni kayıt için 0; EF sonra güncelleyecek

                    tr.Invoice = null;

                    if (tr.GibUserCreditAccountId > 0)
                    {
                        var acc = await _db.Set<GibUserCreditAccount>()
                                           .FindAsync(new object[] { tr.GibUserCreditAccountId }, ct);
                        if (acc != null)
                        {
                            _db.Attach(acc);
                            _db.Entry(acc).State = EntityState.Unchanged;
                            tr.GibUserCreditAccount = acc;
                        }
                    }
                    else
                    {
                        if (tr.GibUserCreditAccount != null)
                        {
                            var acc = tr.GibUserCreditAccount;
                            PropagateUserId(inv, acc);
                            ResetIdentityId(acc);
                            TouchAudit(acc, utcNow);
                            acc.Transactions = null;
                        }
                    }
                }
            }

            // ---------------------------
            // 9) INVOICE temel normalize + toplam
            // ---------------------------
            NormalizeInvoice(inv, utcNow);

            if (inv.Total <= 0)
            {
                var itemsTotal = inv.InvoicesItems.Sum(x => x.Total);
                var taxesTotal = inv.InvoicesTaxes.Sum(x => x.Amount);
                inv.Total = itemsTotal + taxesTotal;
            }

            // Son güvenlik: decimal alanlar
            ValidateInvoiceDecimalRanges(inv);
        }

        // ======================================================
        // Customer Graph Fixer (Customer API çağrıları için)
        // ======================================================
        private async Task FixCustomerGraphAsync(Customer c, DateTimeOffset utcNow, CancellationToken ct)
        {
            if (c == null) return;

            // Temel normalize (Name/Surname/TaxNo + audit)
            NormalizeCustomer(c, utcNow);

            // Koleksiyon init
            c.CustomersGroups ??= new List<CustomersGroup>();
            c.Addresses ??= new List<Address>();

            // CustomersGroups -> Group
            foreach (var cg in c.CustomersGroups)
            {
                PropagateUserId(c, cg);
                TouchAudit(cg, utcNow);
                cg.Customer = null;

                if (cg.Group != null)
                {
                    await AttachOrCreateGroupAsync(cg, ct, utcNow);
                }
                else if (cg.GroupId > 0)
                {
                    var existingGrp = await _db.Set<Group>().FindAsync(new object[] { cg.GroupId }, ct);
                    if (existingGrp != null)
                    {
                        _db.Attach(existingGrp);
                        _db.Entry(existingGrp).State = EntityState.Unchanged;
                        cg.Group = existingGrp;
                    }
                }
            }

            // Addresses
            foreach (var addr in c.Addresses)
            {
                PropagateUserId(c, addr);
                NormalizeAddress(addr, utcNow);
                addr.Customer = null;
            }

            // Customer endpoint'i üzerinden invoice grafı ile uğraşmayalım
            c.Invoices = null;
        }

        // ======================================================
        // Item Graph Fixer (Item API çağrıları için)
        // ======================================================
        private async Task FixItemGraphAsync(Item it, DateTimeOffset utcNow, CancellationToken ct)
        {
            if (it == null) return;

            // Temel normalize (Name/Code/Currency + audit)
            NormalizeItem(it, it.Currency, utcNow);

            // BRAND
            if (it.Brand != null)
            {
                await AttachOrCreateBrandAsync(it, ct, utcNow);
            }
            else if (it.BrandId > 0)
            {
                var existingBrand = await _db.Set<Brand>().FindAsync(new object[] { it.BrandId }, ct);
                if (existingBrand != null)
                {
                    _db.Attach(existingBrand);
                    _db.Entry(existingBrand).State = EntityState.Unchanged;
                    it.Brand = existingBrand;
                }
            }

            // UNIT  → UserId + ShortName'e göre dedupe (IX_Unit_UserId_ShortName için kritik)
            if (it.Unit != null)
            {
                await AttachOrCreateUnitAsync(it, ct, utcNow);
            }
            else if (it.UnitId > 0)
            {
                var existingUnit = await _db.Set<Unit>().FindAsync(new object[] { it.UnitId }, ct);
                if (existingUnit != null)
                {
                    _db.Attach(existingUnit);
                    _db.Entry(existingUnit).State = EntityState.Unchanged;
                    it.Unit = existingUnit;
                }
            }

            // ItemsCategory -> Category
            if (it.ItemsCategories != null)
            {
                foreach (var ic in it.ItemsCategories)
                {
                    PropagateUserId(it, ic);
                    TouchAudit(ic, utcNow);

                    // Item navigation'ı kırıyoruz; ilişki Item.ItemsCategories üzerinden kurulacak
                    ic.Item = null;

                    if (ic.Category != null)
                    {
                        await AttachOrCreateCategoryAsync(ic, ct, utcNow);
                    }
                    else if (ic.CategoryId > 0)
                    {
                        var existingCat = await _db.Set<Category>().FindAsync(new object[] { ic.CategoryId }, ct);
                        if (existingCat != null)
                        {
                            _db.Attach(existingCat);
                            _db.Entry(existingCat).State = EntityState.Unchanged;
                            ic.Category = existingCat;
                        }
                    }
                }
            }

            // ItemsDiscounts – Item ekranından normalde gelmez ama normalize edelim
            if (it.ItemsDiscounts != null)
            {
                foreach (var idisc in it.ItemsDiscounts)
                {
                    PropagateUserId(it, idisc);
                    TouchAudit(idisc, utcNow);

                    idisc.Invoice = null;
                    ResetIdentityId(idisc);
                }
            }

            // Identifiers – barkod / GTIN vs.
            if (it.Identifiers != null)
            {
                foreach (var ident in it.Identifiers)
                {
                    PropagateUserId(it, ident);
                    TouchAudit(ident, utcNow);

                    ident.Item = null;
                    ResetIdentityId(ident);
                }
            }
        }

        /// <summary>
        /// TEntity için EF model metadata'sını kullanarak navigation’ları Include eder.
        /// - Tüm normal navigation’lar (reference & collection) için Include ekler.
        /// - maxDepth kadar alt seviye navigation'ları da (Parent.Child.GrandChild) dahil eder.
        /// Böylece Item, Invoice, Customer, Category vb. tüm entity’ler için otomatik çalışır.
        /// </summary>
        private IQueryable<TEntity> ApplyIncludesForEntity(IQueryable<TEntity> query)
        {
            var entityType = _db.Model.FindEntityType(typeof(TEntity));
            if (entityType == null)
                return query;

            const int maxDepth = 2; // İstersen 1/3 yaparak derinliği değiştirebilirsin.

            var visited = new HashSet<IEntityType>();
            return IncludeNavigationsRecursive(
                query,
                entityType,
                prefix: null,
                depth: maxDepth,
                visited: visited,
                rootEntityType: entityType);
        }

        /// <summary>
        /// Navigation ağacını derinlik kontrollü olarak dolaşıp Include path’leri ekler.
        /// Örn: Item -> ItemsCategories -> Category için
        /// - "ItemsCategories"
        /// - "ItemsCategories.Category"
        /// path’lerini üretir.
        /// </summary>
        private IQueryable<TEntity> IncludeNavigationsRecursive(
    IQueryable<TEntity> query,
    IEntityType entityType,
    string? prefix,
    int depth,
    HashSet<IEntityType> visited,
    IEntityType rootEntityType)
        {
            if (depth <= 0)
                return query;

            // Aynı entity tipini aynı include ağacında ikinci kez işlemeye çalışıyorsak
            // (B -> C -> B gibi) sonsuz döngüye girmemek için burada kesiyoruz.
            if (!visited.Add(entityType))
                return query;

            foreach (var navigation in entityType.GetNavigations())
            {
                var targetEntityType = navigation.TargetEntityType;

                // 🔴 ÖNEMLİ:
                // Root tipe (TEntity) geri dönen navigation'ları alt seviyede atla.
                // Örnekler:
                //  - Item (root) -> Brand -> Items   ❌  (Brand.Items atlanacak)
                //  - Invoice (root) -> Customer -> Invoices  ❌
                //
                // prefix == null => root seviyesindeyiz (Item.Brand, Invoice.Customer gibi).
                // prefix != null => alt seviye (Brand.Items, Customer.Invoices gibi).
                if (targetEntityType == rootEntityType && prefix != null)
                    continue;

                // Bu navigation için Include path'i oluştur:
                //  - root seviye: "Brand", "Unit", "ItemsCategories"
                //  - alt seviye:  "ItemsCategories.Category" vb.
                var path = string.IsNullOrEmpty(prefix)
                    ? navigation.Name
                    : $"{prefix}.{navigation.Name}";

                query = query.Include(path);

                // Alt navigation'ları da (max depth kadar) işlemeye devam et
                query = IncludeNavigationsRecursive(
                    query,
                    targetEntityType,
                    path,
                    depth - 1,
                    visited,
                    rootEntityType);
            }

            return query;
        }


        private static void FixGibFirmGraphForAdd(GibFirm firm, DateTimeOffset utcNow)
        {
            // ✅ Invoice kesinlikle insert edilmesin
            firm.Invoices = null;

            // RowVersion: CREATE’te kesinlikle null
            firm.RowVersion = null;

            // services init
            firm.Services ??= new List<GibFirmService>();

            foreach (var svc in firm.Services)
            {
                // Id>0 gelirse create’de sorun çıkarabilir
                if (svc.Id <= 0) svc.RowVersion = null;

                PropagateUserId(firm, svc);
                TouchAudit(svc, utcNow);

                svc.GibFirm = null;

                svc.Aliases ??= new List<GibFirmServiceAlias>();
                foreach (var al in svc.Aliases)
                {
                    if (al.Id <= 0) al.RowVersion = null;

                    PropagateUserId(firm, al);
                    TouchAudit(al, utcNow);

                    al.Service = null;

                    if (string.IsNullOrWhiteSpace(al.Direction))
                        al.Direction = "SENDER";
                }
            }

            // CreditAccounts init (JS create’de gönderebilir)
            if (firm.CreditAccounts != null)
            {
                foreach (var acc in firm.CreditAccounts)
                {
                    if (acc.Id <= 0) acc.RowVersion = null;

                    PropagateUserId(firm, acc);
                    TouchAudit(acc, utcNow);

                    acc.GibFirm = null;

                    // Transactions
                    if (acc.Transactions != null)
                    {
                        foreach (var tr in acc.Transactions)
                        {
                            if (tr.Id <= 0) tr.RowVersion = null;

                            PropagateUserId(firm, tr);
                            TouchAudit(tr, utcNow);

                            // ✅ Invoice insert edilmesin
                            tr.Invoice = null;
                            tr.InvoiceId = null;

                            tr.GibUserCreditAccount = null;
                        }
                    }
                }
            }
        }

        private async Task<GibFirm> UpdateGibFirmGraphAsync(GibFirm incoming, DateTimeOffset utcNow, CancellationToken ct)
        {
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                try
                {
                    // tracked existing graph
                    var existing = await _db.Set<GibFirm>()
                        .Include(x => x.Services)
                            .ThenInclude(s => s.Aliases)
                        .Include(x => x.CreditAccounts)
                            .ThenInclude(a => a.Transactions)
                        .FirstOrDefaultAsync(x => x.Id == incoming.Id, ct);

                    if (existing == null)
                        throw new Exception("GibFirm bulunamadı.");

                    // ✅ Concurrency: sadece firm rowversion’ı client’tan
                    if (incoming.RowVersion != null && incoming.RowVersion.Length > 0)
                    {
                        _db.Entry(existing).Property("RowVersion").OriginalValue = incoming.RowVersion;
                    }

                    // ✅ Invoice update/insert yok
                    existing.Invoices = null;

                    // scalar map
                    existing.Title = incoming.Title;
                    existing.TaxNo = incoming.TaxNo;
                    existing.TaxOffice = incoming.TaxOffice;
                    existing.TaxOfficeProvince = incoming.TaxOfficeProvince;
                    existing.CommercialRegistrationNo = incoming.CommercialRegistrationNo;
                    existing.MersisNo = incoming.MersisNo;
                    existing.CustomerName = incoming.CustomerName;
                    existing.PersonalFirstName = incoming.PersonalFirstName;
                    existing.PersonalLastName = incoming.PersonalLastName;
                    existing.InstitutionType = incoming.InstitutionType;
                    existing.CustomerRepresentative = incoming.CustomerRepresentative;

                    existing.AddressLine = incoming.AddressLine;
                    existing.City = incoming.City;
                    existing.District = incoming.District;
                    existing.Country = incoming.Country;
                    existing.PostalCode = incoming.PostalCode;
                    existing.Phone = incoming.Phone;
                    existing.Email = incoming.Email;

                    existing.CorporateEmail = incoming.CorporateEmail;
                    existing.KepAddress = incoming.KepAddress;

                    existing.ResponsibleTckn = incoming.ResponsibleTckn;
                    existing.ResponsibleFirstName = incoming.ResponsibleFirstName;
                    existing.ResponsibleLastName = incoming.ResponsibleLastName;
                    existing.ResponsibleMobilePhone = incoming.ResponsibleMobilePhone;
                    existing.ResponsibleEmail = incoming.ResponsibleEmail;

                    existing.CreatedByPersonFirstName = incoming.CreatedByPersonFirstName;
                    existing.CreatedByPersonLastName = incoming.CreatedByPersonLastName;
                    existing.CreatedByPersonMobilePhone = incoming.CreatedByPersonMobilePhone;

                    existing.GibAlias = incoming.GibAlias;
                    existing.ApiKey = incoming.ApiKey;
                    existing.IsEInvoiceRegistered = incoming.IsEInvoiceRegistered;
                    existing.IsEArchiveRegistered = incoming.IsEArchiveRegistered;

                    TouchAudit(existing, utcNow);

                    // ===== Services merge =====
                    incoming.Services ??= new List<GibFirmService>();
                    existing.Services ??= new List<GibFirmService>();

                    // remove missing services
                    var incomingServiceIds = incoming.Services.Where(s => s.Id > 0).Select(s => s.Id).ToHashSet();
                    var removeServices = existing.Services.Where(s => s.Id > 0 && !incomingServiceIds.Contains(s.Id)).ToList();
                    foreach (var rs in removeServices)
                    {
                        if (rs.Aliases != null && rs.Aliases.Count > 0)
                            _db.RemoveRange(rs.Aliases);

                        _db.Remove(rs);
                    }

                    // upsert services
                    foreach (var incSvc in incoming.Services)
                    {
                        if (incSvc.Id > 0)
                        {
                            var exSvc = existing.Services.FirstOrDefault(s => s.Id == incSvc.Id);
                            if (exSvc == null) continue;

                            exSvc.ServiceType = incSvc.ServiceType;
                            exSvc.StartDate = incSvc.StartDate;
                            exSvc.EndDate = incSvc.EndDate;
                            exSvc.TariffType = incSvc.TariffType;
                            exSvc.Status = incSvc.Status;

                            PropagateUserId(existing, exSvc);
                            TouchAudit(exSvc, utcNow);

                            // aliases merge
                            incSvc.Aliases ??= new List<GibFirmServiceAlias>();
                            exSvc.Aliases ??= new List<GibFirmServiceAlias>();

                            var incomingAliasIds = incSvc.Aliases.Where(a => a.Id > 0).Select(a => a.Id).ToHashSet();
                            var removeAliases = exSvc.Aliases.Where(a => a.Id > 0 && !incomingAliasIds.Contains(a.Id)).ToList();
                            foreach (var ra in removeAliases) _db.Remove(ra);

                            foreach (var incAl in incSvc.Aliases)
                            {
                                if (incAl.Id > 0)
                                {
                                    var exAl = exSvc.Aliases.FirstOrDefault(a => a.Id == incAl.Id);
                                    if (exAl == null) continue;

                                    exAl.Direction = string.IsNullOrWhiteSpace(incAl.Direction) ? "SENDER" : incAl.Direction;
                                    exAl.Alias = incAl.Alias;

                                    PropagateUserId(existing, exAl);
                                    TouchAudit(exAl, utcNow);
                                }
                                else
                                {
                                    var newAl = new GibFirmServiceAlias
                                    {
                                        Direction = string.IsNullOrWhiteSpace(incAl.Direction) ? "SENDER" : incAl.Direction,
                                        Alias = incAl.Alias
                                    };
                                    PropagateUserId(existing, newAl);
                                    TouchAudit(newAl, utcNow);
                                    exSvc.Aliases.Add(newAl);
                                }
                            }
                        }
                        else
                        {
                            var newSvc = new GibFirmService
                            {
                                ServiceType = incSvc.ServiceType,
                                StartDate = incSvc.StartDate,
                                EndDate = incSvc.EndDate,
                                TariffType = incSvc.TariffType,
                                Status = incSvc.Status,
                                Aliases = new List<GibFirmServiceAlias>()
                            };

                            PropagateUserId(existing, newSvc);
                            TouchAudit(newSvc, utcNow);

                            incSvc.Aliases ??= new List<GibFirmServiceAlias>();
                            foreach (var incAl in incSvc.Aliases)
                            {
                                var newAl = new GibFirmServiceAlias
                                {
                                    Direction = string.IsNullOrWhiteSpace(incAl.Direction) ? "SENDER" : incAl.Direction,
                                    Alias = incAl.Alias
                                };
                                PropagateUserId(existing, newAl);
                                TouchAudit(newAl, utcNow);
                                newSvc.Aliases.Add(newAl);
                            }

                            existing.Services.Add(newSvc);
                        }
                    }

                    // ===== CreditAccounts: update’te dokunma (UI sadece create’de gönderiyor) =====
                    // incoming.CreditAccounts gönderilse bile burada karıştırmıyoruz.
                    // (istersen ayrıca ayrı endpoint ile yönet.)

                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    return existing;
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    _db.ChangeTracker.Clear();
                    throw;
                }
            });
        }

    }
}
