using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.Services.Implementations
{
    public class TransferLogService : ITransferLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransferLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TransferLog>> GetAllTransferLogsAsync()
        {
            return await _unitOfWork.TransferLogRepository.GetAllAsync();
        }

        public async Task<TransferLog> GetTransferLogByIdAsync(int id)
        {
            return await _unitOfWork.TransferLogRepository.GetByIdAsync(id);
        }

        public async Task<TransferLog> CreateTransferLogAsync(TransferLog transferLog)
        {
            await _unitOfWork.TransferLogRepository.AddAsync(transferLog);
            await _unitOfWork.CommitAsync();
            return transferLog;
        }

        public async Task<TransferLog> UpdateTransferLogAsync(TransferLog transferLog)
        {
            _unitOfWork.TransferLogRepository.Update(transferLog);
            await _unitOfWork.CommitAsync();
            return transferLog;
        }

        public async Task<bool> DeleteTransferLogAsync(int id)
        {
            var transferLog = await GetTransferLogByIdAsync(id);
            if (transferLog == null) return false;

            _unitOfWork.TransferLogRepository.Remove(transferLog);
            await _unitOfWork.CommitAsync();
            return true;
        }
    }
}