using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities.Models;

namespace TurkSoft.Service.Inferfaces
{
    public interface ITigerGlAccountRepository
    {
        Task<List<TigerGlAccount>> SearchAsync(string searchTerm, int take = 30);
    }
}
