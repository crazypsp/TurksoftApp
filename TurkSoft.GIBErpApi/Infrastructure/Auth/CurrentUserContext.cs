using System.Security.Claims;

namespace TurkSoft.GIBErpApi.Infrastructure.Auth
{
    public interface ICurrentUserContext
    {
        long? UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        string[] Roles { get; }
        bool HasRole(string role);
    }

    public sealed class CurrentUserContext : ICurrentUserContext
    {
        private readonly IHttpContextAccessor _http;
        public CurrentUserContext(IHttpContextAccessor http) => _http = http;

        public long? UserId => long.TryParse(GetClaim("uid"), out var id) ? id : null;
        public string? UserName => GetClaim("uname");
        public string? Email => GetClaim("email");

        public string[] Roles =>
            _http.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray() ?? Array.Empty<string>();

        public bool HasRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

        private string? GetClaim(string type) => _http.HttpContext?.User?.FindFirst(type)?.Value;
    }
}
