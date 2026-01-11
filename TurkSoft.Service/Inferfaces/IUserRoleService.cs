using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Services.Interfaces
{
    public interface IUserRoleService
    {
        Task<IEnumerable<UserRole>> GetAllUserRolesAsync();
        Task<UserRole> GetUserRoleByIdAsync(int id);
        Task<UserRole> CreateUserRoleAsync(UserRole userRole);
        Task<UserRole> UpdateUserRoleAsync(UserRole userRole);
        Task<bool> DeleteUserRoleAsync(int id);
    }
}