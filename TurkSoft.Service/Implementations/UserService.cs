using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.UserRepository.GetAllAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _unitOfWork.UserRepository.GetByIdAsync(id);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            var users = await _unitOfWork.UserRepository.FindAsync(u => u.Email == username);
            return users.FirstOrDefault();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var users = await _unitOfWork.UserRepository.FindAsync(u => u.Email == email);
            return users.FirstOrDefault();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            user.CreatedDate = DateTime.UtcNow;
            user.IsActive = true;

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            user.ModifiedDate = DateTime.UtcNow;
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.CommitAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null) return false;

            _unitOfWork.UserRepository.Remove(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null || !user.IsActive) return false;

            // Basit şifre kontrolü (gerçek projede hash kullanın)
            return user.PasswordHash == HashPassword(password, user.PasswordSalt);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;

            if (user.PasswordHash != HashPassword(currentPassword, user.PasswordSalt))
                return false;

            user.PasswordHash = HashPassword(newPassword, user.PasswordSalt);
            user.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;

            user.PasswordHash = HashPassword(newPassword, user.PasswordSalt);
            user.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null) return false;

            user.IsActive = false;
            user.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> ActivateUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null) return false;

            user.IsActive = true;
            user.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        private string HashPassword(string password, string salt)
        {
            // Basit hash örneği - gerçek projede BCrypt veya benzeri kullanın
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var saltedPassword = password + salt;
                var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}