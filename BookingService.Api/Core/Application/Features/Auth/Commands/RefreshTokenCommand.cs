using BookingService.Api.Core.Application.Features.Auth.DTOs;
using BookingService.Api.Core.Domain.Enums;
using BookingService.Api.Infrastructure.Data;
using BookingService.Api.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Auth.Commands;

public record RefreshTokenCommand(RefreshTokenRequest Request, string IpAddress) : IRequest<AuthResponse>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public RefreshTokenCommandHandler(
        ApplicationDbContext context,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _context = context;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Get user ID from expired access token
        var userId = _tokenService.GetUserIdFromToken(request.AccessToken);
        if (userId == null)
        {
            throw new UnauthorizedAccessException("Invalid access token");
        }

        // Find user with refresh tokens
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive");
        }

        // Find the refresh token
        var refreshToken = user.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);
        if (refreshToken == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Check if token is active
        if (!refreshToken.IsActive)
        {
            // If token was revoked, revoke all descendant tokens (token reuse detection)
            if (refreshToken.IsRevoked)
            {
                await RevokeDescendantTokens(refreshToken, user, command.IpAddress, 
                    "Attempted reuse of revoked ancestor token", cancellationToken);
            }
            throw new UnauthorizedAccessException("Refresh token is no longer valid");
        }

        // Rotate refresh token: revoke old and create new
        var newRefreshToken = RotateRefreshToken(refreshToken, command.IpAddress, user.Id);
        user.RefreshTokens.Add(newRefreshToken);

        // Remove old inactive tokens
        RemoveOldRefreshTokens(user);

        // Generate new access token
        var roleName = Enum.GetName(typeof(UserRole), user.Role) ?? user.Role.ToString();
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, roleName);

        await _context.SaveChangesAsync(cancellationToken);

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var accessTokenExpiryMinutes = int.Parse(jwtSettings["AccessTokenExpiryMinutes"] ?? "15");

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes),
            RefreshTokenExpiration = newRefreshToken.ExpiresAt,
            Email = user.Email,
            Name = user.Name,
            Role = roleName
        };
    }

    private Core.Domain.Common.RefreshToken RotateRefreshToken(
        Core.Domain.Common.RefreshToken oldToken, 
        string ipAddress,
        int userId)
    {
        var newRefreshToken = _tokenService.GenerateRefreshToken(userId, ipAddress);
        
        oldToken.RevokedAt = DateTime.UtcNow;
        oldToken.RevokedByIp = ipAddress;
        oldToken.ReasonRevoked = "Replaced by new token";
        oldToken.ReplacedByToken = newRefreshToken.Token;

        return newRefreshToken;
    }

    private async Task RevokeDescendantTokens(
        Core.Domain.Common.RefreshToken token,
        Core.Domain.Common.User user,
        string ipAddress,
        string reason,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(token.ReplacedByToken)) return;

        var childToken = user.RefreshTokens.FirstOrDefault(t => t.Token == token.ReplacedByToken);
        if (childToken == null) return;

        if (childToken.IsActive)
        {
            childToken.RevokedAt = DateTime.UtcNow;
            childToken.RevokedByIp = ipAddress;
            childToken.ReasonRevoked = reason;
        }
        
        await RevokeDescendantTokens(childToken, user, ipAddress, reason, cancellationToken);
    }

    private void RemoveOldRefreshTokens(Core.Domain.Common.User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var refreshTokenTTLDays = int.Parse(jwtSettings["RefreshTokenTTLDays"] ?? "2");

        var tokensToRemove = user.RefreshTokens
            .Where(t => !t.IsActive && 
                        t.CreatedAt.AddDays(refreshTokenTTLDays) <= DateTime.UtcNow)
            .ToList();

        foreach (var token in tokensToRemove)
        {
            user.RefreshTokens.Remove(token);
        }
    }
}
