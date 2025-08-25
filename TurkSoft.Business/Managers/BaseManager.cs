using Microsoft.EntityFrameworkCore;   // EF Core DbContext/DbSet/EF.Property için
using System;                          // Guid / DateTime için
using System.Collections.Generic;      // List<T> için
using System.Threading;                // CancellationToken için
using System.Threading.Tasks;          // Task tabanlı imzalar için
using TurkSoft.Business.Interface;     // IBaseService<T> sözleşmesi için
using TurkSoft.Data.Context;           // AppDbContext için

namespace TurkSoft.Business.Manager
{
    /// <summary>
    /// EF Core tabanlı generic servis implementasyonu.
    /// - T => herhangi bir entity (class)
    /// - Soft-delete: DeleteDate gölge/CLR alanını doldurur, varsa IsActive=false yapar
    /// - Global soft-delete filtresi AppDbContext'te tanımlıdır (DeleteDate == null)
    /// </summary>
    public class BaseManager<T> : IBaseService<T> where T : class
    {
        // DbContext referansı: EF işlemlerinin merkezi
        private readonly AppDbContext _db;

        // T tipi için doğrudan DbSet; Sorgu/Ekleme/Güncelleme/Silme işlemleri buradan yürür
        private readonly DbSet<T> _set;

        /// <summary>
        /// DI (Dependency Injection) ile DbContext enjekte edilir.
        /// </summary>
        public BaseManager(AppDbContext db)
        {
            _db = db;             // Field'a atama
            _set = _db.Set<T>();  // İlgili entity için DbSet elde edilir
        }

        /// <inheritdoc />
        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            // EF.Property<Guid>(e, "Id"): CLR property olmasa bile (shadow) Id alanını güvenle erişir
            return await _set.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, ct);
        }

        /// <inheritdoc />
        public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
        {
            // AsNoTracking: Okuma senaryolarında değişiklik takibini kapatır ⇒ performans kazanımı
            return await _set.AsNoTracking().ToListAsync(ct);
        }

        /// <inheritdoc />
        public async Task<T> AddAsync(T entity, CancellationToken ct = default)
        {
            // Ekleme işlemine entity'i al ve EF değişiklik izleyicisine ekle
            await _set.AddAsync(entity, ct);

            // AppDbContext.SaveChanges* içinde CreateDate/IsActive güvenceye alınır
            await _db.SaveChangesAsync(ct);

            // Persist edilen nesneyi geri döndür
            return entity;
        }

        /// <inheritdoc />
        public async Task<T> UpdateAsync(T entity, CancellationToken ct = default)
        {
            // Var olan kaydı güncelle (değişmiş alanları EF izler)
            _set.Update(entity);

            // AppDbContext.SaveChanges* içinde UpdateDate güncellenir
            await _db.SaveChangesAsync(ct);

            // Güncellenen nesne geri döner
            return entity;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            // Önce silinecek kaydı getir
            var entity = await GetByIdAsync(id, ct);
            if (entity == null) return false; // Yoksa false

            // Soft-delete: DeleteDate alanına UTC zamanını yaz
            _db.Entry(entity).Property<DateTime?>("DeleteDate").CurrentValue = DateTime.UtcNow;

            // Varsa IsActive alanını false yap (shadow/CLR fark etmeksizin)
            var isActiveProp = _db.Entry(entity).Property<bool?>("IsActive");
            if (isActiveProp != null)
                isActiveProp.CurrentValue = false;

            // Değişiklikleri kalıcılaştır
            await _db.SaveChangesAsync(ct);

            return true; // Başarılı
        }
    }
}
