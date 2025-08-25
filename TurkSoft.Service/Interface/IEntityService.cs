using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Service.Interface
{
    /// <summary>
    /// Uygulama servislerinin (domain bazlı servisler) ortak CRUD sözleşmesi.
    /// EF Core tarafındaki entity gereksinimleriyle uyum için T bir referans tip olmalıdır.
    /// </summary>
    /// <typeparam name="T">Servisin yönettiği entity tipi (class olmalı)</typeparam>
    public interface IEntityService<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<T>> GetAllAsync(CancellationToken ct = default);
        Task<T> AddAsync(T entity, CancellationToken ct = default);
        Task<T> UpdateAsync(T entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
