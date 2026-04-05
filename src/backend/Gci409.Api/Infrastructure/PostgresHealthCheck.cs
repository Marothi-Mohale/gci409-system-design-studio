using Gci409.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Gci409.Api.Infrastructure;

public sealed class PostgresHealthCheck(Gci409DbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("PostgreSQL connection is available.")
            : HealthCheckResult.Unhealthy("PostgreSQL connection is unavailable.");
    }
}
