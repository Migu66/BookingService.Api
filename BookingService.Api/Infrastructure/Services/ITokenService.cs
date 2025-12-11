using System.Security.Claims;
using BookingService.Api.Core.Domain.Common;

namespace BookingService.Api.Infrastructure.Services;

public interface ITokenService
{
    /// <summary>
    /// Generates an access token signed with RS256
    /// </summary>
    string GenerateAccessToken(int userId, string email, string role);

    /// <summary>
    /// Generates a cryptographically secure refresh token
    /// </summary>
    RefreshToken GenerateRefreshToken(int userId, string ipAddress);

    /// <summary>
    /// Validates a token and returns the claims principal
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Gets the user ID from a token (even if expired)
    /// </summary>
    int? GetUserIdFromToken(string token);
}
