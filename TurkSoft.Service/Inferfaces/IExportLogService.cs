using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Services.Interfaces
{
    public interface IExportLogService
    {
        Task<IEnumerable<ExportLog>> GetAllExportLogsAsync();
        Task<ExportLog> GetExportLogByIdAsync(int id);
        Task<ExportLog> CreateExportLogAsync(ExportLog exportLog);
        Task<ExportLog> UpdateExportLogAsync(ExportLog exportLog);
        Task<bool> DeleteExportLogAsync(int id);
    }
}