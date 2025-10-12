using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TurkSoft.Data.Context;
using TurkSoft.GIBErpApi.Infrastructure.Auth;

namespace TurkSoft.GibErpApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public sealed class AuthController : ControllerBase
    {
        private readonly GibAppDbContext _db;
        private readonly JwtTokenService _jwt;
        private readonly RefreshTokenService _refresh;

        public AuthController(GibAppDbContext db, JwtTokenService jwt, RefreshTokenService refresh)
        {
            _db = db; _jwt = jwt; _refresh = refresh;
        }

        // ======== DTOs ========
        public sealed class LoginRequest
        {
            public string UsernameOrEmail { get; set; } = default!;
            public string Password { get; set; } = default!;
        }
        public sealed class TokenResponse
        {
            public string AccessToken { get; set; } = default!;
            public DateTime ExpiresAtUtc { get; set; }
            public string RefreshToken { get; set; } = default!;
            public string UserName { get; set; } = default!;
            public string[] Roles { get; set; } = Array.Empty<string>();
        }
        public sealed class RefreshRequest { public string RefreshToken { get; set; } = default!; }

        // ======== kullanıcı + roller ========
        private async Task<(TurkSoft.Entities.GIBEntityDB.Users user, string[] roles)?> FindUserAsync(string identifier)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Username == identifier || x.Email == identifier);
            if (user == null) return null;

            // TODO: Parola doğrulamasını hash ile yap (örnek basit)
            var roles = await (from ur in _db.UserRole
                               join r in _db.Role on ur.RoleId equals r.Id
                               where ur.UserId == user.Id
                               select r.Name).ToArrayAsync();
            return (user, roles);
        }

        // ============== WEB (COOKIE) ==============
        [HttpPost("login-web")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWeb([FromBody] LoginRequest req)
        {
            var found = await FindUserAsync(req.UsernameOrEmail);
            if (found is null) return Unauthorized("Kullanıcı bulunamadı.");
            var (user, roles) = found.Value;
            if (user.Password != req.Password) return Unauthorized("Parola hatalı.");

            var claims = new List<Claim>
            {
                new("uid", user.Id.ToString()),
                new(ClaimTypes.Name, user.Username ?? ""),
                new(ClaimTypes.Email, user.Email ?? "")
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return Ok(new { message = "Web login başarılı.", user = user.Username, roles });
        }

        [HttpPost("logout-web")]
        [Authorize]
        public async Task<IActionResult> LogoutWeb()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return NoContent();
        }

        // ============== API (JWT + REFRESH) ==============
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest req)
        {
            var found = await FindUserAsync(req.UsernameOrEmail);
            if (found is null) return Unauthorized("Kullanıcı bulunamadı.");
            var (user, roles) = found.Value;
            if (user.Password != req.Password) return Unauthorized("Parola hatalı.");

            var (access, exp) = _jwt.CreateToken(user.Id, user.Username ?? "", user.Email ?? "", roles);
            var rt = await _refresh.IssueAsync(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "-", Request.Headers.UserAgent.ToString(), days: 15);

            return Ok(new TokenResponse
            {
                AccessToken = access,
                ExpiresAtUtc = exp,
                RefreshToken = rt.Token,
                UserName = user.Username ?? "",
                Roles = roles
            });
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> Refresh([FromBody] RefreshRequest req)
        {
            var t = await _db.ApiRefreshToken.FirstOrDefaultAsync(x => x.Token == req.RefreshToken);
            if (t is null || !t.IsActive) return Unauthorized("Refresh token geçersiz.");

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == t.UserId);
            if (user is null) return Unauthorized("Kullanıcı bulunamadı.");

            var roles = await (from ur in _db.UserRole
                               join r in _db.Role on ur.RoleId equals r.Id
                               where ur.UserId == user.Id
                               select r.Name).ToArrayAsync();

            // rotation
            await _refresh.RevokeAsync(t, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "-", "rotated");
            var newRt = await _refresh.IssueAsync(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "-", Request.Headers.UserAgent.ToString(), 15);

            var (access, exp) = _jwt.CreateToken(user.Id, user.Username ?? "", user.Email ?? "", roles);
            return Ok(new TokenResponse
            {
                AccessToken = access,
                ExpiresAtUtc = exp,
                RefreshToken = newRt.Token,
                UserName = user.Username ?? "",
                Roles = roles
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
        {
            var t = await _db.ApiRefreshToken.FirstOrDefaultAsync(x => x.Token == req.RefreshToken);
            if (t != null && t.IsActive)
                await _refresh.RevokeAsync(t, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "-", "logout");

            return NoContent();
        }

        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            var uidClaim = User.FindFirst("uid") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (uidClaim == null || !long.TryParse(uidClaim.Value, out var uid)) return Unauthorized();

            await _refresh.RevokeAllForUserAsync(uid, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "-");
            return NoContent();
        }
    }
}
