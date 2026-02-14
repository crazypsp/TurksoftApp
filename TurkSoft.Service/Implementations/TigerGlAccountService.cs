using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities.Models;
using TurkSoft.Service.Inferfaces;

namespace TurkSoft.Service.Implementations
{
    public sealed class TigerGlAccountService : ITigerGlAccountService
    {
        private readonly ITigerGlAccountRepository _repo;

        public TigerGlAccountService(ITigerGlAccountRepository repo)
        {
            _repo = repo;
        }

        public Task<List<TigerGlAccount>> SearchAsync(string searchTerm, int take = 30)
            => _repo.SearchAsync(searchTerm, take);
    }
}
