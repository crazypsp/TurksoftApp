using Microsoft.Extensions.Diagnostics.HealthChecks;
using TurkSoft.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace TurkSoft.ErpApi.HealthChecks
{
    public sealed class DbContextHealthCheck: IHealthCheck
    {
        private readonly AppDbContext _db;
        public DbContextHealthCheck(AppDbContext db) => _db = db;

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var ok = await _db.Database.CanConnectAsync(cancellationToken);
                return ok ? HealthCheckResult.Healthy("Database reachable.")
                          : HealthCheckResult.Unhealthy("Database unreachable.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database check failed.", ex);
            }
        }
    }
}
