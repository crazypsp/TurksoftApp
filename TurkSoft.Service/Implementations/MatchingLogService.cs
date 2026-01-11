using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.Services.Implementations
{
    public class MatchingLogService : IMatchingLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MatchingLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<MatchingLog>> GetAllMatchingLogsAsync()
        {
            return await _unitOfWork.MatchingLogRepository.GetAllAsync();
        }

        public async Task<MatchingLog> GetMatchingLogByIdAsync(int id)
        {
            return await _unitOfWork.MatchingLogRepository.GetByIdAsync(id);
        }

        public async Task<MatchingLog> CreateMatchingLogAsync(MatchingLog matchingLog)
        {
            await _unitOfWork.MatchingLogRepository.AddAsync(matchingLog);
            await _unitOfWork.CommitAsync();
            return matchingLog;
        }

        public async Task<MatchingLog> UpdateMatchingLogAsync(MatchingLog matchingLog)
        {
            _unitOfWork.MatchingLogRepository.Update(matchingLog);
            await _unitOfWork.CommitAsync();
            return matchingLog;
        }

        public async Task<bool> DeleteMatchingLogAsync(int id)
        {
            var matchingLog = await GetMatchingLogByIdAsync(id);
            if (matchingLog == null) return false;

            _unitOfWork.MatchingLogRepository.Remove(matchingLog);
            await _unitOfWork.CommitAsync();
            return true;
        }
    }
}