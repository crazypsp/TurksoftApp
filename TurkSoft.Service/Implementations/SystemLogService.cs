using System;
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
            ArgumentNullException.ThrowIfNull(systemLog);

            // DB constraint’leriyle uyum: NOT NULL alanlar asla null gitmesin
            systemLog.LogLevel = string.IsNullOrWhiteSpace(systemLog.LogLevel) ? "INFO" : systemLog.LogLevel;
            systemLog.Message = systemLog.Message ?? string.Empty;
            systemLog.Source = systemLog.Source ?? string.Empty;
            systemLog.ActionName = systemLog.ActionName ?? string.Empty;
            systemLog.IpAddress = string.IsNullOrWhiteSpace(systemLog.IpAddress) ? "127.0.0.1" : systemLog.IpAddress;

            if (systemLog.CreatedDate == default)
                systemLog.CreatedDate = DateTime.UtcNow;

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
