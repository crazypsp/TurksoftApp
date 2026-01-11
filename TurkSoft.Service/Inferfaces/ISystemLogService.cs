using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Services.Interfaces
{
    public interface ISystemLogService
    {
        Task<IEnumerable<SystemLog>> GetAllSystemLogsAsync();
        Task<SystemLog> GetSystemLogByIdAsync(int id);
        Task<SystemLog> CreateSystemLogAsync(SystemLog systemLog);
        Task<SystemLog> UpdateSystemLogAsync(SystemLog systemLog);
        Task<bool> DeleteSystemLogAsync(int id);
    }
}