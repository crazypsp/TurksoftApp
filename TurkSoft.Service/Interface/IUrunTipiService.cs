// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// UrunTipi entity'sinin tanımlı olduğu namespace (varlık sınıfı)
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    // Kullanıcı işlemleri için servis arayüzü tanımlanıyor
    // Bu arayüz, sadece UrunTipi (User) entity’sine özel CRUD metotlarını içerir
    public interface IUrunTipiService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve UrunTipi nesnesi döner
        Task<UrunTipi> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<UrunTipi> döner
        Task<List<UrunTipi>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen UrunTipi nesnesini geri döner
        Task<UrunTipi> AddAsync(UrunTipi entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş UrunTipi nesnesini döner
        Task<UrunTipi> UpdateAsync(UrunTipi entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}