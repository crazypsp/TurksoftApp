using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities.Models;

namespace TurkSoft.Service.Inferfaces
{
    public interface IClCardRepository
    {
        Task<List<ClCard>> GetAllClCardsAsync();
        Task<ClCard> GetClCardByCodeAsync(string code);
        Task<List<ClCard>> SearchClCardsByNameAsync(string name);
    }
}
