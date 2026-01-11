using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Services.Interfaces
{
    public interface ITransactionImportService
    {
        Task<IEnumerable<TransactionImport>> GetAllImportsAsync();
        Task<TransactionImport> GetImportByIdAsync(int id);
        Task<TransactionImport> CreateImportAsync(TransactionImport transactionImport);
        Task<TransactionImport> UpdateImportAsync(TransactionImport transactionImport);
        Task<bool> DeleteImportAsync(int id);
    }
}