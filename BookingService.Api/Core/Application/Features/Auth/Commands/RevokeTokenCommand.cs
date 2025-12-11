using BookingService.Api.Core.Application.Features.Auth.DTOs;
using BookingService.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Auth.Commands;

public record RevokeTokenCommand(RevokeTokenRequest Request, string IpAddress, int UserId) : IRequest<bool>;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public RevokeTokenCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RevokeTokenCommand command, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user == null)
        {
            return false;
        }

        var refreshToken = user.RefreshTokens.FirstOrDefault(t => t.Token == command.Request.RefreshToken);
        if (refreshToken == null || !refreshToken.IsActive)
        {
            return false;
        }

        // Revoke token
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = command.IpAddress;
        refreshToken.ReasonRevoked = "Revoked by user";

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
