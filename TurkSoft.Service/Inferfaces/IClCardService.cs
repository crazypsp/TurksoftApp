using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities.Models;

namespace TurkSoft.Service.Inferfaces
{
    public interface IClCardService
    {
        Task<List<ClCard>> GetAllCardsAsync();
        Task<ClCard> GetCardByCodeAsync(string code);
        Task<List<ClCard>> SearchCardsAsync(string searchTerm);
        Task<List<ClCard>> GetPagedCardsAsync(int pageNumber, int pageSize);
    }
}
