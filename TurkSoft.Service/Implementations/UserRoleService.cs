using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.Services.Implementations
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserRoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UserRole>> GetAllUserRolesAsync()
        {
            return await _unitOfWork.UserRoleRepository.GetAllAsync();
        }

        public async Task<UserRole> GetUserRoleByIdAsync(int id)
        {
            return await _unitOfWork.UserRoleRepository.GetByIdAsync(id);
        }

        public async Task<UserRole> CreateUserRoleAsync(UserRole userRole)
        {
            await _unitOfWork.UserRoleRepository.AddAsync(userRole);
            await _unitOfWork.CommitAsync();
            return userRole;
        }

        public async Task<UserRole> UpdateUserRoleAsync(UserRole userRole)
        {
            _unitOfWork.UserRoleRepository.Update(userRole);
            await _unitOfWork.CommitAsync();
            return userRole;
        }

        public async Task<bool> DeleteUserRoleAsync(int id)
        {
            var userRole = await GetUserRoleByIdAsync(id);
            if (userRole == null) return false;

            _unitOfWork.UserRoleRepository.Remove(userRole);
            await _unitOfWork.CommitAsync();
            return true;
        }
    }
}