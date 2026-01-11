using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.Services.Implementations
{
    public class ExportLogService : IExportLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExportLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ExportLog>> GetAllExportLogsAsync()
        {
            return await _unitOfWork.ExportLogRepository.GetAllAsync();
        }

        public async Task<ExportLog> GetExportLogByIdAsync(int id)
        {
            return await _unitOfWork.ExportLogRepository.GetByIdAsync(id);
        }

        public async Task<ExportLog> CreateExportLogAsync(ExportLog exportLog)
        {
            await _unitOfWork.ExportLogRepository.AddAsync(exportLog);
            await _unitOfWork.CommitAsync();
            return exportLog;
        }

        public async Task<ExportLog> UpdateExportLogAsync(ExportLog exportLog)
        {
            _unitOfWork.ExportLogRepository.Update(exportLog);
            await _unitOfWork.CommitAsync();
            return exportLog;
        }

        public async Task<bool> DeleteExportLogAsync(int id)
        {
            var exportLog = await GetExportLogByIdAsync(id);
            if (exportLog == null) return false;

            _unitOfWork.ExportLogRepository.Remove(exportLog);
            await _unitOfWork.CommitAsync();
            return true;
        }
    }
}