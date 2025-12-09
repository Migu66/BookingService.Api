using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace BookingService.Api.Infrastructure.HealthChecks;

public class SystemHealthCheck : IHealthCheck
{
    private readonly ILogger<SystemHealthCheck> _logger;

    public SystemHealthCheck(ILogger<SystemHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64;
            var privateMemory = process.PrivateMemorySize64;
            var cpuTime = process.TotalProcessorTime;
            var threadCount = process.Threads.Count;

            var workingSetMB = workingSet / 1024.0 / 1024.0;
            var privateMemoryMB = privateMemory / 1024.0 / 1024.0;

            var data = new Dictionary<string, object>
            {
                { "working_set_mb", Math.Round(workingSetMB, 2) },
                { "private_memory_mb", Math.Round(privateMemoryMB, 2) },
                { "cpu_time", cpuTime.ToString() },
                { "thread_count", threadCount },
                { "uptime", (DateTime.Now - process.StartTime).ToString(@"dd\.hh\:mm\:ss") },
                { "environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production" },
                { "os", Environment.OSVersion.ToString() },
                { "dotnet_version", Environment.Version.ToString() }
            };

            // Check if memory usage is too high (threshold: 500 MB)
            if (workingSetMB > 500)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    "High memory usage detected",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                "System resources are healthy",
                data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "System health check failed",
                ex,
                data: new Dictionary<string, object>
                {
                    { "error", ex.Message }
                }));
        }
    }
}
