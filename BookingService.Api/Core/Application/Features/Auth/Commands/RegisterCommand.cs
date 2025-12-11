using BookingService.Api.Core.Application.Features.Auth.DTOs;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Core.Domain.Enums;
using BookingService.Api.Infrastructure.Data;
using BookingService.Api.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Auth.Commands;

public record RegisterCommand(RegisterRequest Request, string IpAddress) : IRequest<AuthResponse>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public RegisterCommandHandler(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<AuthResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Create new user
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var roleName = Enum.GetName(typeof(UserRole), user.Role) ?? user.Role.ToString();
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, roleName);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id, command.IpAddress);

        // Save refresh token
        user.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var accessTokenExpiryMinutes = int.Parse(jwtSettings["AccessTokenExpiryMinutes"] ?? "15");

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes),
            RefreshTokenExpiration = refreshToken.ExpiresAt,
            Email = user.Email,
            Name = user.Name,
            Role = roleName
        };
    }
}
