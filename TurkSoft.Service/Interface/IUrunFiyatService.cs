// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// UrunFiyat entity'sinin tanımlı olduğu namespace (varlık sınıfı)
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    // Kullanıcı işlemleri için servis arayüzü tanımlanıyor
    // Bu arayüz, sadece UrunFiyat (User) entity’sine özel CRUD metotlarını içerir
    public interface IUrunFiyatService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve UrunFiyat nesnesi döner
        Task<UrunFiyat> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<UrunFiyat> döner
        Task<List<UrunFiyat>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen UrunFiyat nesnesini geri döner
        Task<UrunFiyat> AddAsync(UrunFiyat entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş UrunFiyat nesnesini döner
        Task<UrunFiyat> UpdateAsync(UrunFiyat entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}