using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    public interface IKeyAccountService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve KeyAccount nesnesi döner
        Task<KeyAccount> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<KeyAccount> döner
        Task<List<KeyAccount>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen KeyAccount nesnesini geri döner
        Task<KeyAccount> AddAsync(KeyAccount entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş KeyAccount nesnesini döner
        Task<KeyAccount> UpdateAsync(KeyAccount entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}
