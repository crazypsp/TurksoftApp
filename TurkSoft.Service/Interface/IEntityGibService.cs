using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Service.Interface
{
    /// <summary>
    /// GIB veritabanındaki entity'ler için generic servis sözleşmesi.
    /// Id tipleri farklı (int, long, Guid vs.) olabileceği için TKey parametresi tanımlanmıştır.
    /// </summary>
    /// <typeparam name="TEntity">İşlemlerin yapılacağı entity tipi</typeparam>
    /// <typeparam name="TKey">Entity'nin Id tipi (ör. int, long, Guid...)</typeparam>
    public interface IEntityGibService<TEntity, TKey> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);
        Task<List<TEntity>> GetAllAsync(CancellationToken ct = default);
        Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(TKey id, CancellationToken ct = default);
    }
}
