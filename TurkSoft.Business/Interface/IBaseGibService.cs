using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Business.Interface
{
    /// <summary>
    /// GIB veritabanı entity'leri için generic servis sözleşmesi.
    /// Not: GIB modellerinde Id tipleri (long/int/short) farklı olabildiği için TKey ile genellenmiştir.
    /// </summary>
    public interface IBaseGibService<TEntity, TKey> where TEntity : class
    {
        /// <summary>Verilen Id'ye göre tekil kaydı döndürür. Bulunamazsa null.</summary>
        Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);

        /// <summary>Tüm kayıtları (varsa IsActive==true filtreli) listeler.</summary>
        Task<List<TEntity>> GetAllAsync(CancellationToken ct = default);

        /// <summary>Yeni bir kayıt ekler ve eklenen nesneyi döndürür.</summary>
        Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);

        /// <summary>Var olan kaydı günceller ve döndürür.</summary>
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken ct = default);

        /// <summary>
        /// Silme: Entity'de bool IsActive varsa soft-delete (IsActive=false), yoksa fiziksel silme yapar.
        /// Başarılıysa true döner.
        /// </summary>
        Task<bool> DeleteAsync(TKey id, CancellationToken ct = default);
    }
}
