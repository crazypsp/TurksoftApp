using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Services.Interfaces
{
    public interface ITransferLogService
    {
        Task<IEnumerable<TransferLog>> GetAllTransferLogsAsync();
        Task<TransferLog> GetTransferLogByIdAsync(int id);
        Task<TransferLog> CreateTransferLogAsync(TransferLog transferLog);
        Task<TransferLog> UpdateTransferLogAsync(TransferLog transferLog);
        Task<bool> DeleteTransferLogAsync(int id);
    }
}