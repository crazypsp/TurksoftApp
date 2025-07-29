// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// WhatsappAyar entity'sinin tanımlı olduğu namespace (varlık sınıfı)
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    // Kullanıcı işlemleri için servis arayüzü tanımlanıyor
    // Bu arayüz, sadece WhatsappAyar (User) entity’sine özel CRUD metotlarını içerir
    public interface IWhatsappAyarService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve WhatsappAyar nesnesi döner
        Task<WhatsappAyar> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<WhatsappAyar> döner
        Task<List<WhatsappAyar>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen WhatsappAyar nesnesini geri döner
        Task<WhatsappAyar> AddAsync(WhatsappAyar entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş WhatsappAyar nesnesini döner
        Task<WhatsappAyar> UpdateAsync(WhatsappAyar entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}