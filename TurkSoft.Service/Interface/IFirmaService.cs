// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Firma entity'sinin tanımlı olduğu namespace (varlık sınıfı)
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    // Kullanıcı işlemleri için servis arayüzü tanımlanıyor
    // Bu arayüz, sadece Firma (User) entity’sine özel CRUD metotlarını içerir
    public interface IFirmaService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve Firma nesnesi döner
        Task<Firma> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<Firma> döner
        Task<List<Firma>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen Firma nesnesini geri döner
        Task<Firma> AddAsync(Firma entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş Firma nesnesini döner
        Task<Firma> UpdateAsync(Firma entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}