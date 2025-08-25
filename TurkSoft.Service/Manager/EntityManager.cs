using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Business.Interface;
using TurkSoft.Service.Interface;

namespace TurkSoft.Service.Manager
{
    /// <summary>
    /// Tüm entity'ler için ortak service manager.
    /// İçeride IBaseService<T> kullanır; EF Core ile uyum için T bir referans tip olmalıdır.
    /// </summary>
    public class EntityManager<T> : IEntityService<T> where T : class
    {
        protected readonly IBaseService<T> _base;

        public EntityManager(IBaseService<T> baseBusiness)
        {
            _base = baseBusiness;
        }

        public Task<T> AddAsync(T entity, CancellationToken ct = default) => _base.AddAsync(entity, ct);
        public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) => _base.DeleteAsync(id, ct);
        public Task<List<T>> GetAllAsync(CancellationToken ct = default) => _base.GetAllAsync(ct);
        public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) => _base.GetByIdAsync(id, ct);
        public Task<T> UpdateAsync(T entity, CancellationToken ct = default) => _base.UpdateAsync(entity, ct);
    }
}
