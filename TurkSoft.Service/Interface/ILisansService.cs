// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Lisans entity'sinin tanımlı olduğu namespace (varlık sınıfı)
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    // Kullanıcı işlemleri için servis arayüzü tanımlanıyor
    // Bu arayüz, sadece Lisans (User) entity’sine özel CRUD metotlarını içerir
    public interface ILisansService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve Lisans nesnesi döner
        Task<Lisans> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<Lisans> döner
        Task<List<Lisans>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen Lisans nesnesini geri döner
        Task<Lisans> AddAsync(Lisans entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş Lisans nesnesini döner
        Task<Lisans> UpdateAsync(Lisans entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}