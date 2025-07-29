// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Bu interface, iş katmanında kullanılacak genel servis şablonlarını içerir
namespace TurkSoft.Business.Interface
{
    // Generic bir interface tanımlanıyor. T, class tipinde olmak zorunda.
    // Bu sayede tüm entity'ler için tekrar tekrar CRUD yazmak yerine bu şablon kullanılabilir.
    public interface IBaseService<T> where T : class
    {
        // Verilen GUID (id) değerine göre T tipinde (örneğin User, Firma vs.) bir nesne döner.
        // Asenkron çalışır, yani işlem tamamlandığında sonucu döner.
        Task<T> GetByIdAsync(Guid id);

        // Veritabanındaki tüm T tipindeki nesneleri listeler.
        // Örneğin tüm kullanıcıları, tüm firmaları vs.
        Task<List<T>> GetAllAsync();

        // Verilen T tipindeki (örneğin yeni bir kullanıcı) nesneyi veritabanına ekler.
        // Eklenen nesneyi geri döner.
        Task<T> AddAsync(T entity);

        // Verilen T nesnesini günceller.
        // Güncellenmiş haliyle geri döner.
        Task<T> UpdateAsync(T entity);

        // Verilen GUID'e göre bir nesneyi siler.
        // Silme işlemi başarılıysa true, değilse false döner.
        Task<bool> DeleteAsync(Guid id);
    }
}