// Sistem kütüphanelerini projeye dahil ediyoruz.
// Bu kütüphaneler Task, List, Guid gibi temel sınıf ve yapılara erişim sağlar.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Uygulamanın iş katmanında tanımlı olan generic servis arayüzü.
// IBaseService<T> tüm varlıklar (entity) için genel CRUD işlemlerini tanımlar.
using TurkSoft.Business.Interface;

// WhatsappAyar adındaki Entity (veritabanı tablosunu temsil eden sınıf).
using TurkSoft.Entities.EntityDB;


// Kullanıcıya özel servis interface'i.
// Bu sınıfın uygulayacağı metodları tanımlar (Contract = Sözleşme).
using TurkSoft.Service.Interface;

namespace TurkSoft.Service.Manager
{
    // Bu sınıf, kullanıcıya özel servislerin yöneticisidir (service manager).
    // IWhatsappAyarService arayüzünü (interface) implement ederek, bu interface'de tanımlanan tüm metodları somut olarak uygular.
    public class WhatsappAyarManager : IWhatsappAyarService
    {
        // BaseManager'dan gelen generic servis sınıfı burada kullanılmak üzere private readonly olarak tanımlanır.
        // Tüm CRUD işlemleri bu servis üzerinden yönlendirilir.
        private readonly IBaseService<WhatsappAyar> _baseBusiness;

        // Constructor (yapıcı metod), dışarıdan bu sınıfa IBaseService<WhatsappAyar> türünde bir bağımlılık (dependency) enjekte eder.
        // Dependency Injection (DI) prensibi sayesinde loosely-coupled (gevşek bağlı) bir yapı elde edilir.
        public WhatsappAyarManager(IBaseService<WhatsappAyar> baseBusiness)
        {
            _baseBusiness = baseBusiness;
        }

        // Yeni bir kullanıcıyı veritabanına ekleyen metot.
        // IEntity tipinde aldığı nesneyi base servis aracılığıyla veritabanına kaydeder.
        // Asenkron çalışır, çünkü veritabanı işlemi zaman alabilir ve UI'ı (arayüzü) bloklamaması gerekir.
        public async Task<WhatsappAyar> AddAsync(WhatsappAyar entity)
        {
            // BaseManager içindeki AddAsync metodunu çağırıyoruz.
            return await _baseBusiness.AddAsync(entity);
        }

        // Belirli bir ID'ye sahip kullanıcıyı siler.
        // Soft delete prensibine göre çalışır (veritabanından tamamen silmez, sadece IsActive = false yapar).
        public async Task<bool> DeleteAsync(Guid id)
        {
            // BaseManager'daki DeleteAsync metodunu çağırır.
            return await _baseBusiness.DeleteAsync(id);
        }

        // Tüm aktif kullanıcıları veritabanından asenkron şekilde çeker.
        public async Task<List<WhatsappAyar>> GetAllAsync()
        {
            // BaseManager'daki GetAllAsync metodu çağrılır.
            return await _baseBusiness.GetAllAsync();
        }

        // ID'ye göre tek bir kullanıcıyı getirir.
        // Hem ID eşleşmesi hem de IsActive == true koşulu base'de kontrol edilir.
        public async Task<WhatsappAyar> GetByIdAsync(Guid id)
        {
            return await _baseBusiness.GetByIdAsync(id);
        }

        // Var olan bir kullanıcıyı günceller.
        // Güncellenmiş kullanıcı nesnesi geri döner.
        public async Task<WhatsappAyar> UpdateAsync(WhatsappAyar entity)
        {
            return await _baseBusiness.UpdateAsync(entity);
        }
    }
}