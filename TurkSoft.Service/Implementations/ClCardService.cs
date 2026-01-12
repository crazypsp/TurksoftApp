using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities.Models;
using TurkSoft.Service.Inferfaces;

namespace TurkSoft.Service.Implementations
{
    public class ClCardService : IClCardService
    {
        private readonly IClCardRepository _clCardRepository;

        public ClCardService(IClCardRepository clCardRepository)
        {
            _clCardRepository = clCardRepository;
        }

        public async Task<List<ClCard>> GetAllCardsAsync()
        {
            return await _clCardRepository.GetAllClCardsAsync();
        }

        public async Task<ClCard> GetCardByCodeAsync(string code)
        {
            return await _clCardRepository.GetClCardByCodeAsync(code);
        }

        public async Task<List<ClCard>> SearchCardsAsync(string searchTerm)
        {
            return await _clCardRepository.SearchClCardsByNameAsync(searchTerm);
        }

        public async Task<List<ClCard>> GetPagedCardsAsync(int pageNumber, int pageSize)
        {
            var allCards = await _clCardRepository.GetAllClCardsAsync();
            return allCards
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}
