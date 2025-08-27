// Middleware/GlobalExceptionMiddleware.cs
using Microsoft.AspNetCore.Mvc;

namespace TurkSoft.ErpApi.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogDebug("Request cancelled by client. Path: {Path}", context.Request.Path);
                return; // 499 benzeri kod YAZMA!
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                var problem = new ProblemDetails
                {
                    Title = "Beklenmeyen bir hata oluştu.",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = ex.Message,
                    Instance = context.TraceIdentifier
                };

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = problem.Status ?? 500;
                await context.Response.WriteAsJsonAsync(problem);
            }
        }
    }
}
