using System;                         // Guid tipi ve temel sistem tipleri için
using System.Collections.Generic;     // List<T> için
using System.Threading;               // CancellationToken için
using System.Threading.Tasks;         // Task tabanlı asenkron imzalar için

namespace TurkSoft.Business.Interface
{
    /// <summary>
    /// Tüm entity'ler için ortak CRUD işlevlerini tanımlayan generic servis sözleşmesi.
    /// Bu arayüz; veri erişim katmanıyla iş katmanı arasında arabulucu görevi görür.
    /// </summary>
    /// <typeparam name="T">İşlemlerin yapılacağı entity tipi (ör. Firma, Satis...)</typeparam>
    public interface IBaseService<T> where T : class
    {
        /// <summary>
        /// Verilen Id'ye göre tekil kaydı döndürür. Kaydı bulamazsa null döner.
        /// </summary>
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Tüm aktif (soft-delete uygulanmamış) kayıtları listeler.
        /// </summary>
        Task<List<T>> GetAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Yeni bir kayıt ekler ve eklenen nesneyi döndürür.
        /// </summary>
        Task<T> AddAsync(T entity, CancellationToken ct = default);

        /// <summary>
        /// Var olan bir kaydı günceller ve güncellenen nesneyi döndürür.
        /// </summary>
        Task<T> UpdateAsync(T entity, CancellationToken ct = default);

        /// <summary>
        /// Soft delete uygular (DeleteDate alanını doldurur, varsa IsActive=false yapar).
        /// Başarılıysa true, değilse false döner.
        /// </summary>
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
