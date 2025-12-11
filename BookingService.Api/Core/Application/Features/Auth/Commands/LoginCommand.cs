using AutoMapper;
using BookingService.Api.Core.Application.Features.Auth.DTOs;
using BookingService.Api.Core.Domain.Enums;
using BookingService.Api.Infrastructure.Data;
using BookingService.Api.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Auth.Commands;

public record LoginCommand(LoginRequest Request, string IpAddress) : IRequest<AuthResponse>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(
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

    public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Find user by email
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Remove old inactive refresh tokens (keep only active ones)
        var inactiveTokens = user.RefreshTokens.Where(t => !t.IsActive).ToList();
        foreach (var token in inactiveTokens)
        {
            _context.RefreshTokens.Remove(token);
        }

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