// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// SmsGonderim entity'sinin tanımlı olduğu namespace (varlık sınıfı)
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    // Kullanıcı işlemleri için servis arayüzü tanımlanıyor
    // Bu arayüz, sadece SmsGonderim (User) entity’sine özel CRUD metotlarını içerir
    public interface ISmsGonderimService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve SmsGonderim nesnesi döner
        Task<SmsGonderim> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<SmsGonderim> döner
        Task<List<SmsGonderim>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen SmsGonderim nesnesini geri döner
        Task<SmsGonderim> AddAsync(SmsGonderim entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş SmsGonderim nesnesini döner
        Task<SmsGonderim> UpdateAsync(SmsGonderim entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}