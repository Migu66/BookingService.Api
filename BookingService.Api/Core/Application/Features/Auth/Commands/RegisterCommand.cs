using AutoMapper;
using BookingService.Api.Core.Application.Features.Auth.DTOs;
using BookingService.Api.Core.Domain.Common;
using BookingService.Api.Core.Domain.Enums;
using BookingService.Api.Infrastructure.Data;
using BookingService.Api.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Core.Application.Features.Auth.Commands;

public record RegisterCommand(RegisterRequest Request) : IRequest<AuthResponse>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(
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

        // Generate token
        var response = _mapper.Map<AuthResponse>(user);
        response.Token = _tokenService.GenerateToken(user.Id, user.Email, user.Role.ToString());

        return response;
    }
}
