using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Turksoft.BankServiceAPI.Security
{
    public sealed class ApiKeyMiddleware : IMiddleware
    {
        private readonly ApiKeyOptions _options;

        public ApiKeyMiddleware(IOptions<ApiKeyOptions> options)
        {
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Key config edilmemişse server hatası
            if (string.IsNullOrWhiteSpace(_options.Key))
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Server ApiKey is not configured.");
                return;
            }

            var headerName = string.IsNullOrWhiteSpace(_options.HeaderName) ? "X-API-KEY" : _options.HeaderName;

            if (!context.Request.Headers.TryGetValue(headerName, out var provided) ||
                string.IsNullOrWhiteSpace(provided))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync($"Unauthorized: Missing '{headerName}' header.");
                return;
            }

            // Sabit zamanlı karşılaştırma (timing attack riskini azaltır)
            if (!FixedTimeEquals(provided.ToString(), _options.Key))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Invalid API Key.");
                return;
            }

            await next(context);
        }

        private static bool FixedTimeEquals(string a, string b)
        {
            var ba = Encoding.UTF8.GetBytes(a);
            var bb = Encoding.UTF8.GetBytes(b);

            if (ba.Length != bb.Length) return false;
            return CryptographicOperations.FixedTimeEquals(ba, bb);
        }
    }
}
