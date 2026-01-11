using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _unitOfWork.RoleRepository.GetAllAsync();
        }

        public async Task<Role> GetRoleByIdAsync(int id)
        {
            return await _unitOfWork.RoleRepository.GetByIdAsync(id);
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            await _unitOfWork.RoleRepository.AddAsync(role);
            await _unitOfWork.CommitAsync();
            return role;
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            _unitOfWork.RoleRepository.Update(role);
            await _unitOfWork.CommitAsync();
            return role;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await GetRoleByIdAsync(id);
            if (role == null) return false;

            _unitOfWork.RoleRepository.Remove(role);
            await _unitOfWork.CommitAsync();
            return true;
        }
    }
}