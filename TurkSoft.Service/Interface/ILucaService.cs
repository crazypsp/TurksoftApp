using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Service.Interface
{
    public interface ILucaService
    {
        // Belirli bir kullanıcıyı ID (Guid) bilgisine göre getirir
        // Asenkron çalışır ve Luca nesnesi döner
        Task<Luca> GetByIdAsync(Guid id);

        // Tüm kullanıcıları listeleyen metot
        // Asenkron çalışır ve List<Luca> döner
        Task<List<Luca>> GetAllAsync();

        // Yeni bir kullanıcı ekler
        // Eklenen Luca nesnesini geri döner
        Task<Luca> AddAsync(Luca entity);

        // Var olan kullanıcıyı günceller
        // Güncellenmiş Luca nesnesini döner
        Task<Luca> UpdateAsync(Luca entity);

        // Kullanıcıyı siler (tercihen soft delete uygulanır)
        // Başarı durumuna göre true/false döner
        Task<bool> DeleteAsync(Guid id);
    }
}
