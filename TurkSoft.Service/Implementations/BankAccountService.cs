using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.Services.Implementations
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BankAccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<BankAccount>> GetAllBankAccountsAsync()
        {
            return await _unitOfWork.BankAccountRepository.GetAllAsync();
        }

        public async Task<BankAccount> GetBankAccountByIdAsync(int id)
        {
            return await _unitOfWork.BankAccountRepository.GetByIdAsync(id);
        }

        public async Task<BankAccount> CreateBankAccountAsync(BankAccount bankAccount)
        {
            await _unitOfWork.BankAccountRepository.AddAsync(bankAccount);
            await _unitOfWork.CommitAsync();
            return bankAccount;
        }

        public async Task UpdateBankAccountAsync(BankAccount bankAccount)
        {
            _unitOfWork.BankAccountRepository.Update(bankAccount);
            await _unitOfWork.CommitAsync();
        }

        public async Task DeleteBankAccountAsync(int id)
        {
            var bankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(id);
            if (bankAccount != null)
            {
                _unitOfWork.BankAccountRepository.Remove(bankAccount);
                await _unitOfWork.CommitAsync();
            }
        }
    }
}