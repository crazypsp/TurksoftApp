using Microsoft.EntityFrameworkCore;
using TurkSoft.Data.GibData;
using TurkSoft.Entities.GIBEntityDB;

namespace TurkSoft.GIBErpApi.Infrastructure.Auth
{
    public sealed class RefreshTokenService
    {
        private readonly GibAppDbContext _db;
        public RefreshTokenService(GibAppDbContext db) => _db = db;

        public async Task<ApiRefreshToken> IssueAsync(long userId, string ip, string? userAgent, int days)
        {
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                              .Replace("+", "").Replace("/", "").Replace("=", "");

            var entity = new ApiRefreshToken
            {
                UserId = userId,
                Token = token,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedByIp = ip,
                UserAgent = userAgent,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(days)
            };
            _db.ApiRefreshToken.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<ApiRefreshToken?> GetActiveAsync(long userId, string token) =>
            await _db.ApiRefreshToken.FirstOrDefaultAsync(x =>
                x.UserId == userId && x.Token == token && x.IsActive);

        public async Task RevokeAsync(ApiRefreshToken t, string ip, string? reason = null)
        {
            t.RevokedAtUtc = DateTime.UtcNow;
            t.RevokedByIp = ip;
            await _db.SaveChangesAsync();
        }

        public async Task RevokeAllForUserAsync(long userId, string ip)
        {
            var list = await _db.ApiRefreshToken.Where(x => x.UserId == userId && x.RevokedAtUtc == null).ToListAsync();
            foreach (var t in list)
            {
                t.RevokedAtUtc = DateTime.UtcNow;
                t.RevokedByIp = ip;
            }
            await _db.SaveChangesAsync();
        }
    }
}
