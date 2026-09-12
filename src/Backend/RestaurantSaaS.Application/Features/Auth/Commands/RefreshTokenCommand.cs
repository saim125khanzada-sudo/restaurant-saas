using RestaurantSaaS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.SharedKernel.Common;

namespace RestaurantSaaS.Application.Features.Auth.Commands;

public record RefreshTokenCommand(
    string RefreshToken,
    string DeviceFingerprint,
    string IpAddress,
    string UserAgent
) : IRequest<Result<LoginResponse>>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtService;

    public RefreshTokenCommandHandler(IApplicationDbContext context, IJwtTokenService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _jwtService.HashToken(request.RefreshToken);

        var session = await _context.DeviceSessions
            .Include(s => s.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == tokenHash, cancellationToken);

        if (session == null || session.IsRevoked || session.ExpiresAt < DateTimeOffset.UtcNow)
        {
            return Result<LoginResponse>.Failure("Invalid or expired session. Please log in again.");
        }

        var user = session.User;

        // Rotate Refresh Token
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newRefreshTokenHash = _jwtService.HashToken(newRefreshToken);

        session.IsRevoked = true;
        session.RevokedAt = DateTimeOffset.UtcNow;
        session.ReplacedByTokenHash = newRefreshTokenHash;

        var newSession = new DeviceSession
        {
            RestaurantId = user.RestaurantId,
            UserId = user.Id,
            RefreshTokenHash = newRefreshTokenHash,
            DeviceFingerprint = request.DeviceFingerprint,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedBy = user.Email
        };

        _context.DeviceSessions.Add(newSession);
        await _context.SaveChangesAsync(cancellationToken);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var newAccessToken = _jwtService.GenerateAccessToken(user, roles, permissions);

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
            AccessToken: newAccessToken,
            RefreshToken: newRefreshToken,
            ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(15),
            User: userDto
        ));
    }
}

