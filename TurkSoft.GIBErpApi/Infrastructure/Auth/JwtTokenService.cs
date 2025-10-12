using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TurkSoft.GIBErpApi.Infrastructure.Auth
{
    public sealed class JwtTokenService
    {
        private readonly JwtOptions _opt;
        private readonly SymmetricSecurityKey _key;

        public JwtTokenService(IOptions<JwtOptions> opt)
        {
            _opt = opt.Value;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SigningKey));
        }

        public (string token, DateTime expiresAtUtc) CreateToken(long userId, string userName, string email, string[] roles)
        {
            var claims = new List<Claim>
            {
                new("uid", userId.ToString()),
                new("uname", userName ?? ""),
                new("email", email ?? "")
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_opt.AccessTokenMinutes);

            var jwt = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
        }
    }
}
