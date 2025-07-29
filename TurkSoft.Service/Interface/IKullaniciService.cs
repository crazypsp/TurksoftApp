// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Kullanici entity'sinin tanımlı olduğu namespace (varlık sınıfı)
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    // Kullanıcı işlemleri için servis arayüzü tanımlanıyor
    // Bu arayüz, sadece Kullanici (User) entity’sine özel CRUD metotlarını içerir
    public interface IKullaniciService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve Kullanici nesnesi döner
        Task<Kullanici> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<Kullanici> döner
        Task<List<Kullanici>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen Kullanici nesnesini geri döner
        Task<Kullanici> AddAsync(Kullanici entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş Kullanici nesnesini döner
        Task<Kullanici> UpdateAsync(Kullanici entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}