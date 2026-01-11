using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Services.Interfaces
{
    public interface IBankAccountService
    {
        Task<IEnumerable<BankAccount>> GetAllBankAccountsAsync();
        Task<BankAccount> GetBankAccountByIdAsync(int id);
        Task<BankAccount> CreateBankAccountAsync(BankAccount bankAccount);
        Task UpdateBankAccountAsync(BankAccount bankAccount);
        Task DeleteBankAccountAsync(int id);
    }

}