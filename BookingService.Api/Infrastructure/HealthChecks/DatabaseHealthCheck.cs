using BookingService.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BookingService.Api.Infrastructure.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;

    public DatabaseHealthCheck(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy(
                    "Cannot connect to the database",
                    data: new Dictionary<string, object>
                    {
                        { "database", "PostgreSQL" }
                    });
            }

            var usersCount = await _context.Users.CountAsync(cancellationToken);
            var resourcesCount = await _context.Resources.CountAsync(cancellationToken);
            var reservationsCount = await _context.Reservations.CountAsync(cancellationToken);
            var blockedTimesCount = await _context.BlockedTimes.CountAsync(cancellationToken);

            var data = new Dictionary<string, object>
            {
                { "database", "PostgreSQL" },
                { "server", _context.Database.GetDbConnection().DataSource },
                { "database_name", _context.Database.GetDbConnection().Database },
                { "total_users", usersCount },
                { "total_resources", resourcesCount },
                { "total_reservations", reservationsCount },
                { "total_blocked_times", blockedTimesCount }
            };

            return HealthCheckResult.Healthy(
                "Database is healthy and responsive",
                data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database health check failed",
                ex,
                data: new Dictionary<string, object>
                {
                    { "error", ex.Message }
                });
        }
    }
}
