using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.Services.Implementations
{
    public class TransactionImportService : ITransactionImportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionImportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TransactionImport>> GetAllImportsAsync()
        {
            return await _unitOfWork.TransactionImportRepository.GetAllAsync();
        }

        public async Task<TransactionImport> GetImportByIdAsync(int id)
        {
            return await _unitOfWork.TransactionImportRepository.GetByIdAsync(id);
        }

        public async Task<TransactionImport> CreateImportAsync(TransactionImport transactionImport)
        {
            await _unitOfWork.TransactionImportRepository.AddAsync(transactionImport);
            await _unitOfWork.CommitAsync();
            return transactionImport;
        }

        public async Task<TransactionImport> UpdateImportAsync(TransactionImport transactionImport)
        {
            _unitOfWork.TransactionImportRepository.Update(transactionImport);
            await _unitOfWork.CommitAsync();
            return transactionImport;
        }

        public async Task<bool> DeleteImportAsync(int id)
        {
            var transactionImport = await GetImportByIdAsync(id);
            if (transactionImport == null) return false;

            _unitOfWork.TransactionImportRepository.Remove(transactionImport);
            await _unitOfWork.CommitAsync();
            return true;
        }
    }
}