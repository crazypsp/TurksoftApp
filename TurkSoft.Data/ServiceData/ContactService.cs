using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Data.ServiceData
{
    public class ContactService
    {
        private readonly ApplicationDbContext _context;

        public ContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tüm contact'ları getir
        public async Task<List<Tm.Contact>> GetAllContactsAsync()
        {
            return await _context.Contacts.ToListAsync();
        }

        // ID'ye göre contact getir
        public async Task<Tm.Contact> GetContactByIdAsync(int id)
        {
            return await _context.Contacts.FindAsync(id);
        }

        // Yeni contact ekle
        public async Task<Tm.Contact> AddContactAsync(Tm.Contact contact)
        {
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return contact;
        }

        // Contact güncelle
        public async Task<Tm.Contact> UpdateContactAsync(Tm.Contact contact)
        {
            _context.Entry(contact).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return contact;
        }

        // Contact sil
        public async Task<bool> DeleteContactAsync(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
                return false;

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
            return true;
        }

        // Özel sorgu örneği
        public async Task<List<Tm.Contact>> GetActiveContactsAsync()
        {
            return await _context.Contacts
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
