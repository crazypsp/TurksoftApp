// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// SmsAyar entity'sinin tanımlı olduğu namespace (varlık sınıfı)
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    // Kullanıcı işlemleri için servis arayüzü tanımlanıyor
    // Bu arayüz, sadece SmsAyar (User) entity’sine özel CRUD metotlarını içerir
    public interface ISmsAyarService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve SmsAyar nesnesi döner
        Task<SmsAyar> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<SmsAyar> döner
        Task<List<SmsAyar>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen SmsAyar nesnesini geri döner
        Task<SmsAyar> AddAsync(SmsAyar entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş SmsAyar nesnesini döner
        Task<SmsAyar> UpdateAsync(SmsAyar entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}