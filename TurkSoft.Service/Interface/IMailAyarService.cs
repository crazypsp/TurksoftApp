// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// MailAyar entity'sinin tanımlı olduğu namespace (varlık sınıfı)
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    // Kullanıcı işlemleri için servis arayüzü tanımlanıyor
    // Bu arayüz, sadece MailAyar (User) entity’sine özel CRUD metotlarını içerir
    public interface IMailAyarService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve MailAyar nesnesi döner
        Task<MailAyar> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<MailAyar> döner
        Task<List<MailAyar>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen MailAyar nesnesini geri döner
        Task<MailAyar> AddAsync(MailAyar entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş MailAyar nesnesini döner
        Task<MailAyar> UpdateAsync(MailAyar entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}