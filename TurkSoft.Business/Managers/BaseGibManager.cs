using Microsoft.EntityFrameworkCore;   
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TurkSoft.Business.Interface;
using TurkSoft.Data.Context;

namespace TurkSoft.Business.Managers
{
    /// <summary>
    /// GIB DbContext üzerinde çalışan generic servis implementasyonu.
    /// - Id alan adı "Id" varsayılır.
    /// - Id tipi (TKey) entity sınıflarına göre (long/int/short/Guid) generic tanımlanmıştır.
    /// - Okumalarda varsa IsActive==true filtresi otomatik uygulanır.
    /// - Silmede IsActive mevcutsa soft-delete, yoksa fiziksel silme yapılır.
    /// </summary>
    public class BaseGibManager<TEntity, TKey> : IBaseGibService<TEntity, TKey>
        where TEntity : class
    {
        private readonly GibAppDbContext _db;
        private readonly DbSet<TEntity> _set;

        private static readonly PropertyInfo IdProp = typeof(TEntity).GetProperty("Id")
            ?? throw new InvalidOperationException($"'{typeof(TEntity).Name}' üzerinde 'Id' alanı bulunamadı.");

        private static readonly PropertyInfo? IsActiveProp = typeof(TEntity).GetProperty("IsActive");

        public BaseGibManager(GibAppDbContext db)
        {
            _db = db;
            _set = _db.Set<TEntity>();
        }

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
            await _set.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
            return entity;
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
                // Soft delete
                IsActiveProp.SetValue(entity, false);
            }
            else
            {
                // Fiziksel silme
                _set.Remove(entity);
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }

        // === Helpers ===

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
                    $"Id tür dönüşümü başarısız: {typeof(TKey).Name} -> {IdProp.PropertyType.Name}", ex);
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
    }
}
