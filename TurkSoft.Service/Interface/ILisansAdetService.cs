using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    internal interface ILisansAdetService
    {
    }
}// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// LisansAdet entity'sinin tanımlı olduğu namespace (varlık sınıfı)
using TurkSoft.Entities;

namespace TurkSoft.Service.Interface
{
    // Kullanıcı işlemleri için servis arayüzü tanımlanıyor
    // Bu arayüz, sadece LisansAdet (User) entity’sine özel CRUD metotlarını içerir
    public interface ILisansAdetService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve LisansAdet nesnesi döner
        Task<LisansAdet> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<LisansAdet> döner
        Task<List<LisansAdet>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen LisansAdet nesnesini geri döner
        Task<LisansAdet> AddAsync(LisansAdet entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş LisansAdet nesnesini döner
        Task<LisansAdet> UpdateAsync(LisansAdet entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}