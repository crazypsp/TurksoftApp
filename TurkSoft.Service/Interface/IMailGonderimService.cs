// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// MailGonderim entity'sinin tanımlı olduğu namespace (varlık sınıfı)
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    // Kullanıcı işlemleri için servis arayüzü tanımlanıyor
    // Bu arayüz, sadece MailGonderim (User) entity’sine özel CRUD metotlarını içerir
    public interface IMailGonderimService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve MailGonderim nesnesi döner
        Task<MailGonderim> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<MailGonderim> döner
        Task<List<MailGonderim>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen MailGonderim nesnesini geri döner
        Task<MailGonderim> AddAsync(MailGonderim entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş MailGonderim nesnesini döner
        Task<MailGonderim> UpdateAsync(MailGonderim entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}