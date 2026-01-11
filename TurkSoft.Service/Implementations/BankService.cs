using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.Services.Implementations
{
    public class BankService : IBankService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BankService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Bank>> GetAllBanksAsync()
        {
            return await _unitOfWork.BankRepository.GetAllAsync();
        }

        public async Task<Bank> GetBankByIdAsync(int id)
        {
            return await _unitOfWork.BankRepository.GetByIdAsync(id);
        }

        public async Task<Bank> CreateBankAsync(Bank bank)
        {
            await _unitOfWork.BankRepository.AddAsync(bank);
            await _unitOfWork.CommitAsync();
            return bank;
        }

        public async Task UpdateBankAsync(Bank bank)
        {
            _unitOfWork.BankRepository.Update(bank);
            await _unitOfWork.CommitAsync();
        }

        public async Task DeleteBankAsync(int id)
        {
            var bank = await _unitOfWork.BankRepository.GetByIdAsync(id);
            if (bank != null)
            {
                _unitOfWork.BankRepository.Remove(bank);
                await _unitOfWork.CommitAsync();
            }
        }
    }
}