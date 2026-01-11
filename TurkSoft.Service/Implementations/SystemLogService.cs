using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.Services.Implementations
{
    public class SystemLogService : ISystemLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SystemLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<SystemLog>> GetAllSystemLogsAsync()
        {
            return await _unitOfWork.SystemLogRepository.GetAllAsync();
        }

        public async Task<SystemLog> GetSystemLogByIdAsync(int id)
        {
            return await _unitOfWork.SystemLogRepository.GetByIdAsync(id);
        }

        public async Task<SystemLog> CreateSystemLogAsync(SystemLog systemLog)
        {
            await _unitOfWork.SystemLogRepository.AddAsync(systemLog);
            await _unitOfWork.CommitAsync();
            return systemLog;
        }

        public async Task<SystemLog> UpdateSystemLogAsync(SystemLog systemLog)
        {
            _unitOfWork.SystemLogRepository.Update(systemLog);
            await _unitOfWork.CommitAsync();
            return systemLog;
        }

        public async Task<bool> DeleteSystemLogAsync(int id)
        {
            var systemLog = await GetSystemLogByIdAsync(id);
            if (systemLog == null) return false;

            _unitOfWork.SystemLogRepository.Remove(systemLog);
            await _unitOfWork.CommitAsync();
            return true;
        }
    }
}