// Entity Framework Core kütüphanesi, DbContext ve DbSet gibi yapıları kullanmak için gereklidir
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// Uygulamaya özel DbContext sınıfı (veri tabanı bağlantısı ve DbSet'ler burada tanımlanır)
using TurkSoft.Data.Context;
// Tüm varlıkların (entity) temel sınıfı BaseEntity burada tanımlı
// Generic servis arayüzü (interface) burada tanımlı
using TurkSoft.Business.Interface;
using TurkSoft.Entities.EntityDB;

namespace TurkSoft.Business.Managers
{
    // BaseManager sınıfı, IBaseService<T> arayüzünü implement eder
    // T: BaseEntity'den türeyen bir sınıf olmalıdır (where T : BaseEntity)
    public class BaseManager<T> : IBaseService<T> where T : BaseEntity
    {
        // protected tanımlanan context, bu sınıftan türetilen alt sınıflar tarafından da erişilebilir
        protected readonly AppDbContext _context;

        // Generic DbSet tanımı; örneğin DbSet<User> gibi çalışır
        private readonly DbSet<T> _dbSet;

        // Constructor metodu: DI (Dependency Injection) ile AppDbContext sınıfı enjekte edilir
        public BaseManager(AppDbContext context)
        {
            _context = context;

            // Generic tipteki DbSet nesnesi context üzerinden alınır
            _dbSet = _context.Set<T>();
        }

        // Yeni bir kayıt eklemek için kullanılan asenkron metot
        public async Task<T> AddAsync(T entity)
        {
            // DbSet üzerinden nesne eklenir
            await _dbSet.AddAsync(entity);

            // Değişiklikler veritabanına kaydedilir
            await _context.SaveChangesAsync();

            // Eklenen nesne geri döner
            return entity;
        }

        // Verilen id’ye sahip kaydı silmek için kullanılır (soft delete mantığıyla)
        public async Task<bool> DeleteAsync(Guid id)
        {
            // İlgili id’ye sahip kayıt bulunur
            var entity = await _dbSet.FindAsync(id);

            // Kayıt bulunamazsa false döner
            if (entity == null) return false;

            // Soft delete işlemi: IsActive false yapılır ve silinme tarihi atanır
            entity.IsActive = false;
            entity.DeleteDate = DateTime.Now;

            // Değişiklikler veritabanına kaydedilir
            await _context.SaveChangesAsync();

            return true;
        }

        // Tüm aktif kayıtları listeleyen metot
        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet
                .AsNoTracking()         // Performans için takip edilmeden sorgulanır (read-only)
                .Where(x => x.IsActive) // Sadece aktif kayıtlar getirilir
                .ToListAsync();         // Liste olarak döndürülür
        }

        // Belirli bir id'ye sahip olan ve aktif olan kaydı getiren metot
        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .AsNoTracking()                         // Performans amaçlı tracking kapalı
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive); // ID’ye ve aktifliğe göre filtreleme
        }

        // Mevcut bir nesneyi güncelleyen metot
        public async Task<T> UpdateAsync(T entity)
        {
            // Güncellenme zamanı güncellenir
            entity.UpdateDate = DateTime.Now;

            // Entity güncellenir
            _dbSet.Update(entity);

            // Değişiklikler veritabanına kaydedilir
            await _context.SaveChangesAsync();

            // Güncellenmiş nesne geri döner
            return entity;
        }
    }
}