using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BookingService.Api.Infrastructure.HealthChecks;

public class JwtConfigurationHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public JwtConfigurationHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = jwtSettings["ExpiryMinutes"];

            var issues = new List<string>();

            if (string.IsNullOrWhiteSpace(secretKey))
                issues.Add("SecretKey is missing");
            else if (secretKey.Length < 32)
                issues.Add("SecretKey is too short (minimum 32 characters)");

            if (string.IsNullOrWhiteSpace(issuer))
                issues.Add("Issuer is missing");

            if (string.IsNullOrWhiteSpace(audience))
                issues.Add("Audience is missing");

            if (string.IsNullOrWhiteSpace(expiryMinutes))
                issues.Add("ExpiryMinutes is missing");

            var data = new Dictionary<string, object>
            {
                { "issuer", issuer ?? "Not configured" },
                { "audience", audience ?? "Not configured" },
                { "expiry_minutes", expiryMinutes ?? "Not configured" },
                { "secret_key_length", secretKey?.Length ?? 0 }
            };

            if (issues.Any())
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"JWT configuration issues: {string.Join(", ", issues)}",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                "JWT configuration is valid",
                data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "JWT configuration health check failed",
                ex,
                data: new Dictionary<string, object>
                {
                    { "error", ex.Message }
                }));
        }
    }
}
