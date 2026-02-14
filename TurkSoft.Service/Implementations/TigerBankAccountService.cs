using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities.Models;
using TurkSoft.Service.Inferfaces;

namespace TurkSoft.Service.Implementations
{
    public sealed class TigerBankAccountService : ITigerBankAccountService
    {
        private readonly ITigerBankAccountRepository _repo;

        public TigerBankAccountService(ITigerBankAccountRepository repo)
        {
            _repo = repo;
        }

        public Task<List<TigerBankAccount>> SearchAsync(string searchTerm, int take = 30)
            => _repo.SearchAsync(searchTerm, take);
    }
}
