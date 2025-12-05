using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BookingService.Api.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(int userId, string email, string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var issuer = jwtSettings["Issuer"] ?? "BookingService";
        var audience = jwtSettings["Audience"] ?? "BookingServiceClient";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var claims = new[]
             {
       new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
     new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
   new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
 };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
      audience: audience,
    claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
       signingCredentials: credentials
      );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
