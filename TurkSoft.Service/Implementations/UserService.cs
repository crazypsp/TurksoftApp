using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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

            var salt = user.PasswordSalt ?? "";
            var newHash = HashPassword(password, salt);

            // ✅ Geriye dönük uyumluluk: eski kayıtlar (SHA256(password + salt)) ile oluşturulmuş olabilir.
            var legacyHash = HashPasswordLegacy(password, salt);

            var okNew = string.Equals(user.PasswordHash, newHash, StringComparison.Ordinal);
            var okLegacy = !okNew && string.Equals(user.PasswordHash, legacyHash, StringComparison.Ordinal);

            // Eski hash ile giriş başarılıysa, yeni formata upgrade et
            if (okLegacy)
            {
                user.PasswordHash = newHash;
                user.ModifiedDate = DateTime.UtcNow;
                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.CommitAsync();
            }

            return okNew || okLegacy;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;

            if (string.IsNullOrWhiteSpace(user.PasswordSalt))
                user.PasswordSalt = Guid.NewGuid().ToString("N");

            var salt = user.PasswordSalt ?? "";
            var okNew = user.PasswordHash == HashPassword(currentPassword, salt);
            var okLegacy = !okNew && user.PasswordHash == HashPasswordLegacy(currentPassword, salt);

            if (!okNew && !okLegacy)
                return false;

            user.PasswordHash = HashPassword(newPassword, salt);
            user.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;

            if (string.IsNullOrWhiteSpace(user.PasswordSalt))
                user.PasswordSalt = Guid.NewGuid().ToString("N");

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

        private static string HashPassword(string password, string salt)
        {
            // Deterministik: SHA256( password + "|" + salt ) => Base64
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes($"{password}|{salt}");
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private static string HashPasswordLegacy(string password, string salt)
        {
            // ✅ Eski format (UsersController'da bir dönem kullanılan): SHA256(password + salt)
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes($"{password}{salt}");
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}