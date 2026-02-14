using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities.Models;

namespace TurkSoft.Service.Inferfaces
{
    public interface ITigerBankAccountRepository
    {
        Task<List<TigerBankAccount>> SearchAsync(string searchTerm, int take = 30);
    }
}
