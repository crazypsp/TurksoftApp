// Gerekli sistem kütüphaneleri
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// MaliMusavir entity'sinin tanımlı olduğu namespace (varlık sınıfı)
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    // Kullanıcı işlemleri için servis arayüzü tanımlanıyor
    // Bu arayüz, sadece MaliMusavir (User) entity’sine özel CRUD metotlarını içerir
    public interface IMaliMusavirService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve MaliMusavir nesnesi döner
        Task<MaliMusavir> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<MaliMusavir> döner
        Task<List<MaliMusavir>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen MaliMusavir nesnesini geri döner
        Task<MaliMusavir> AddAsync(MaliMusavir entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş MaliMusavir nesnesini döner
        Task<MaliMusavir> UpdateAsync(MaliMusavir entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}