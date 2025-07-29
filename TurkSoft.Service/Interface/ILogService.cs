// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Log entity'sinin tanımlı olduğu namespace (varlık sınıfı)
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    // Kullanıcı işlemleri için servis arayüzü tanımlanıyor
    // Bu arayüz, sadece Log (User) entity’sine özel CRUD metotlarını içerir
    public interface ILogService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve Log nesnesi döner
        Task<Log> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<Log> döner
        Task<List<Log>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen Log nesnesini geri döner
        Task<Log> AddAsync(Log entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş Log nesnesini döner
        Task<Log> UpdateAsync(Log entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}