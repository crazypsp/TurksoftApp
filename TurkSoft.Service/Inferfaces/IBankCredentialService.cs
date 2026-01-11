using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Services.Interfaces
{
    public interface IBankCredentialService
    {
        Task<IEnumerable<BankCredential>> GetAllCredentialsAsync();
        Task<BankCredential> GetCredentialByIdAsync(int id);
        Task<BankCredential> CreateCredentialAsync(BankCredential credential);
        Task<BankCredential> UpdateCredentialAsync(BankCredential credential);
        Task<bool> DeleteCredentialAsync(int id);
    }
}