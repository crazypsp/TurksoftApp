using Microsoft.EntityFrameworkCore;   // EF Core DbContext/DbSet için
using System;                          // Guid / DateTime için
using System.Collections.Generic;      // List<T> için
using System.Linq;                     // Where/FirstOrDefault için
using System.Threading;                // CancellationToken için
using System.Threading.Tasks;          // Task tabanlı imzalar için
using TurkSoft.Business.Interface;     // IBaseService<T> sözleşmesi için
using TurkSoft.Data.Context;           // AppDbContext için
using TurkSoft.Entities.EntityDB;      // BaseEntity için

namespace TurkSoft.Business.Manager
{
    /// <summary>
    /// EF Core tabanlı generic servis implementasyonu.
    /// Yalnızca IsActive == true olan kayıtları okur.
    /// Soft-delete: DeleteDate set edilir, IsActive=false yapılır.
    /// </summary>
    public class BaseManager<T> : IBaseService<T> where T : BaseEntity
    {
        private readonly AppDbContext _db;
        private readonly DbSet<T> _set;

        public BaseManager(AppDbContext db)
        {
            _db = db;
            _set = _db.Set<T>();
        }

        /// <inheritdoc />
        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            // Sadece aktif kaydı getir (global filtre varsa DeleteDate zaten elenir)
            return await _set
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id && e.IsActive, ct);
            // Eğer global soft-delete filtresi yoksa, şu şekilde de yazabilirsiniz:
            // .FirstOrDefaultAsync(e => e.Id == id && e.IsActive && e.DeleteDate == null, ct);
        }

        /// <inheritdoc />
        public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
        {
            // Yalnızca aktif kayıtlar
            return await _set
                .AsNoTracking()
                .Where(e => e.IsActive)               // aktif
                                                      //.Where(e => e.DeleteDate == null)    // global filtre yoksa açın
                .ToListAsync(ct);
        }

        /// <inheritdoc />
        public async Task<T> AddAsync(T entity, CancellationToken ct = default)
        {
            await _set.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        /// <inheritdoc />
        public async Task<T> UpdateAsync(T entity, CancellationToken ct = default)
        {
            _set.Update(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _set.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (entity == null) return false;

            // Soft-delete
            entity.DeleteDate = DateTime.UtcNow;
            entity.IsActive = false;

            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
