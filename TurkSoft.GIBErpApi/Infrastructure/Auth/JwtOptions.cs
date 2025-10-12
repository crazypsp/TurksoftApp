namespace TurkSoft.GIBErpApi.Infrastructure.Auth
{
    public sealed class JwtOptions
    {
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public string SigningKey { get; set; } = default!;
        public int AccessTokenMinutes { get; set; } = 60;
        public int RefreshTokenDays { get; set; } = 15;
    }
}
