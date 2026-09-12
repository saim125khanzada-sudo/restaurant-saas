using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.SharedKernel.Common;

namespace RestaurantSaaS.Application.Features.Auth.Commands;

public record LoginCommand(
    string Email,
    string Password,
    string? MfaCode,
    string DeviceFingerprint,
    string IpAddress,
    string UserAgent
) : IRequest<Result<LoginResponse>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtService;
    private readonly IMfaService _mfaService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtService,
        IMfaService mfaService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _mfaService = mfaService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user == null)
        {
            return Result<LoginResponse>.Failure("Invalid email or password.");
        }

        // Account Lockout check
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            return Result<LoginResponse>.Failure($"Account locked due to excessive failed attempts. Try again after {user.LockoutEnd.Value.ToLocalTime():HH:mm:ss}.");
        }

        if (user.Status != UserStatus.Active)
        {
            return Result<LoginResponse>.Failure("Account is not active. Please contact your restaurant administrator.");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15);
                user.FailedLoginAttempts = 0;
            }
            await _context.SaveChangesAsync(cancellationToken);
            return Result<LoginResponse>.Failure("Invalid email or password.");
        }

        // Reset failed login counter upon valid password
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;

        // MFA verification check
        if (user.IsMfaEnabled)
        {
            if (string.IsNullOrWhiteSpace(request.MfaCode))
            {
                return Result<LoginResponse>.Success(new LoginResponse(
                    RequiresMfa: true,
                    AccessToken: null,
                    RefreshToken: null,
                    ExpiresAt: null,
                    User: null
                ));
            }

            if (string.IsNullOrWhiteSpace(user.MfaSecret) || !_mfaService.VerifyTotp(user.MfaSecret, request.MfaCode))
            {
                return Result<LoginResponse>.Failure("Invalid 2FA authentication code.");
            }
        }

        // Extract Roles and Permissions
        var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        // Generate Tokens
        var accessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenHash = _jwtService.HashToken(refreshToken);

        var session = new DeviceSession
        {
            RestaurantId = user.RestaurantId,
            UserId = user.Id,
            RefreshTokenHash = refreshTokenHash,
            DeviceFingerprint = request.DeviceFingerprint,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedBy = user.Email
        };

        user.LastLoginAt = DateTimeOffset.UtcNow;
        _context.DeviceSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(
            user.Id,
            user.RestaurantId,
            user.BranchId,
            user.Email,
            user.FullName,
            user.IsMfaEnabled,
            roles,
            permissions
        );

        return Result<LoginResponse>.Success(new LoginResponse(
            RequiresMfa: false,
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(15),
            User: userDto
        ));
    }
}
