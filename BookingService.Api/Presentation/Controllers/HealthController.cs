using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;
using Microsoft.AspNetCore.RateLimiting;

namespace BookingService.Api.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("default")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Comprehensive health check endpoint with detailed information
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get()
    {
        try
        {
            var report = await _healthCheckService.CheckHealthAsync();

            var response = new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow,
                version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
                application = "BookingService API",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds,
                    tags = e.Value.Tags,
                    data = e.Value.Data,
                    exception = e.Value.Exception?.Message
                }).OrderBy(x => x.name),
                totalDuration = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
                summary = new
                {
                    total = report.Entries.Count,
                    healthy = report.Entries.Count(e => e.Value.Status == HealthStatus.Healthy),
                    degraded = report.Entries.Count(e => e.Value.Status == HealthStatus.Degraded),
                    unhealthy = report.Entries.Count(e => e.Value.Status == HealthStatus.Unhealthy)
                }
            };

            var statusCode = report.Status switch
            {
                HealthStatus.Healthy => StatusCodes.Status200OK,
                HealthStatus.Degraded => StatusCodes.Status200OK,
                HealthStatus.Unhealthy => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status503ServiceUnavailable
            };

            if (statusCode == StatusCodes.Status503ServiceUnavailable)
            {
                _logger.LogWarning("Health check failed with status: {Status}", report.Status);
            }

            return StatusCode(statusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check endpoint failed");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "Unhealthy",
                timestamp = DateTime.UtcNow,
                error = "Health check service is unavailable",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Readiness probe - checks if the application is ready to serve traffic
    /// Returns only critical dependencies (database, configuration)
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Ready()
    {
        try
        {
            var report = await _healthCheckService.CheckHealthAsync(
                check => check.Tags.Contains("ready"));

            var response = new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow,
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    duration = Math.Round(e.Value.Duration.TotalMilliseconds, 2)
                })
            };

            return report.Status == HealthStatus.Healthy
                ? Ok(response)
                : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "Unhealthy",
                timestamp = DateTime.UtcNow,
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Liveness probe - checks if the application is alive and running
    /// Simple check without dependencies
    /// </summary>
    [HttpGet("live")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Live()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            message = "Application is alive and running"
        });
    }

    /// <summary>
    /// Startup probe - checks if the application has started successfully
    /// </summary>
    [HttpGet("startup")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Startup()
    {
        try
        {
            var report = await _healthCheckService.CheckHealthAsync();
            
            return report.Status == HealthStatus.Healthy
                ? Ok(new { status = "Started", timestamp = DateTime.UtcNow })
                : StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    status = "Starting",
                    timestamp = DateTime.UtcNow,
                    message = "Application is still starting up"
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Startup check failed");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "Starting",
                timestamp = DateTime.UtcNow,
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get health metrics in Prometheus format (optional)
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Metrics()
    {
        var report = await _healthCheckService.CheckHealthAsync();

        var metrics = new
        {
            application_health_status = report.Status == HealthStatus.Healthy ? 1 : 0,
            application_health_duration_ms = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                healthy = e.Value.Status == HealthStatus.Healthy ? 1 : 0,
                duration_ms = Math.Round(e.Value.Duration.TotalMilliseconds, 2),
                data = e.Value.Data
            })
        };

        return Ok(metrics);
    }
}


