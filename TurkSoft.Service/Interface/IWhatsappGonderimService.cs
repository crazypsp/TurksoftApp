// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// WhatsappGonderim entity'sinin tanımlı olduğu namespace (varlık sınıfı)
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    // Kullanıcı işlemleri için servis arayüzü tanımlanıyor
    // Bu arayüz, sadece WhatsappGonderim (User) entity’sine özel CRUD metotlarını içerir
    public interface IWhatsappGonderimService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve WhatsappGonderim nesnesi döner
        Task<WhatsappGonderim> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<WhatsappGonderim> döner
        Task<List<WhatsappGonderim>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen WhatsappGonderim nesnesini geri döner
        Task<WhatsappGonderim> AddAsync(WhatsappGonderim entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş WhatsappGonderim nesnesini döner
        Task<WhatsappGonderim> UpdateAsync(WhatsappGonderim entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}