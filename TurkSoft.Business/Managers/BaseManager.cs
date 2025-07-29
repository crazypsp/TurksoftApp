using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Data.Context;
using TurkSoft.Entities;
using TurkSoft.Business.Interface;

namespace TurkSoft.Business.Managers
{
    public class BaseManager<T> : IBaseService<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public BaseManager(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity=await _dbSet.FindAsync(id);
            if(entity==null)return false;
            entity.IsActive = false;
            entity.DeleteDate=DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        }

        public async Task<T> UpdateAsync(T entity)
        {
            entity.UpdateDate = DateTime.Now;
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
