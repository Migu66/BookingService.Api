using AutoMapper;
using BookingService.Api.Core.Application.Features.Auth.DTOs;
using BookingService.Api.Core.Domain.Enums;
using BookingService.Api.Infrastructure.Data;
using BookingService.Api.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Auth.Commands;

public record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public LoginCommandHandler(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IMapper mapper)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Find user by email
        var user = await _context.Users
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

        // Generate token (use enum name if possible so Role claim is "Admin"/"User")
        var response = _mapper.Map<AuthResponse>(user);
        var roleName = Enum.GetName(typeof(UserRole), user.Role) ?? user.Role.ToString();
        response.Token = _tokenService.GenerateToken(user.Id, user.Email, roleName);

        return response;
    }
}