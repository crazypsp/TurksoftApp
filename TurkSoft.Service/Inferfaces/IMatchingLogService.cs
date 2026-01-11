using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Services.Interfaces
{
    public interface IMatchingLogService
    {
        Task<IEnumerable<MatchingLog>> GetAllMatchingLogsAsync();
        Task<MatchingLog> GetMatchingLogByIdAsync(int id);
        Task<MatchingLog> CreateMatchingLogAsync(MatchingLog matchingLog);
        Task<MatchingLog> UpdateMatchingLogAsync(MatchingLog matchingLog);
        Task<bool> DeleteMatchingLogAsync(int id);
    }
}