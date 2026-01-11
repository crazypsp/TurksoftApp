using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.Services.Implementations
{
    public class BankCredentialService : IBankCredentialService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BankCredentialService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<BankCredential>> GetAllCredentialsAsync()
        {
            return await _unitOfWork.BankCredentialRepository.GetAllAsync();
        }

        public async Task<BankCredential> GetCredentialByIdAsync(int id)
        {
            return await _unitOfWork.BankCredentialRepository.GetByIdAsync(id);
        }

        public async Task<BankCredential> CreateCredentialAsync(BankCredential credential)
        {
            await _unitOfWork.BankCredentialRepository.AddAsync(credential);
            await _unitOfWork.CommitAsync();
            return credential;
        }

        public async Task<BankCredential> UpdateCredentialAsync(BankCredential credential)
        {
            _unitOfWork.BankCredentialRepository.Update(credential);
            await _unitOfWork.CommitAsync();
            return credential;
        }

        public async Task<bool> DeleteCredentialAsync(int id)
        {
            var credential = await GetCredentialByIdAsync(id);
            if (credential == null) return false;

            _unitOfWork.BankCredentialRepository.Remove(credential);
            await _unitOfWork.CommitAsync();
            return true;
        }
    }
}