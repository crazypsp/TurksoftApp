// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Paket entity'sinin tanımlı olduğu namespace (varlık sınıfı)
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    // Kullanıcı işlemleri için servis arayüzü tanımlanıyor
    // Bu arayüz, sadece Paket (User) entity’sine özel CRUD metotlarını içerir
    public interface IPaketService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve Paket nesnesi döner
        Task<Paket> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<Paket> döner
        Task<List<Paket>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen Paket nesnesini geri döner
        Task<Paket> AddAsync(Paket entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş Paket nesnesini döner
        Task<Paket> UpdateAsync(Paket entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}