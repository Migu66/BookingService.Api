using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using BookingService.Api.Core.Domain.Common;
using Microsoft.IdentityModel.Tokens;

namespace BookingService.Api.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly RSA _rsa;

    public TokenService(IConfiguration configuration, RSA rsa)
    {
        _configuration = configuration;
        _rsa = rsa;
    }

    public string GenerateAccessToken(int userId, string email, string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var issuer = jwtSettings["Issuer"] ?? "BookingService";
        var audience = jwtSettings["Audience"] ?? "BookingServiceClient";
        var expiryMinutes = int.Parse(jwtSettings["AccessTokenExpiryMinutes"] ?? "15");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var key = new RsaSecurityKey(_rsa);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(int userId, string ipAddress)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var refreshTokenExpiryDays = int.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7");

        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new RsaSecurityKey(_rsa),
                ClockSkew = TimeSpan.Zero
            };

            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }

    public int? GetUserIdFromToken(string token)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // Allow expired tokens for refresh
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new RsaSecurityKey(_rsa)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the RSA public key in PEM format (useful for external validation)
    /// </summary>
    public string GetPublicKeyPem()
    {
        return _rsa.ExportSubjectPublicKeyInfoPem();
    }
}
