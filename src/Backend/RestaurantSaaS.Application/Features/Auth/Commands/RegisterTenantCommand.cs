using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.SharedKernel.Common;

namespace RestaurantSaaS.Application.Features.Auth.Commands;

public record RegisterTenantCommand(
    RegisterTenantAdminRequest Request,
    string IpAddress,
    string UserAgent
) : IRequest<Result<UserDto>>;

public class RegisterTenantCommandHandler : IRequestHandler<RegisterTenantCommand, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterTenantCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UserDto>> Handle(RegisterTenantCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        var normalizedEmail = req.ContactEmail.Trim().ToUpperInvariant();
        var normalizedCode = req.RestaurantCode.Trim().ToUpperInvariant();

        if (await _context.Restaurants.AnyAsync(r => r.Code == normalizedCode, cancellationToken))
        {
            return Result<UserDto>.Failure($"Restaurant with code '{req.RestaurantCode}' already exists.");
        }

        if (await _context.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken))
        {
            return Result<UserDto>.Failure($"A user with email '{req.ContactEmail}' is already registered.");
        }

        var restaurant = new Restaurant
        {
            Name = req.RestaurantName.Trim(),
            Code = normalizedCode,
            ContactEmail = req.ContactEmail.Trim(),
            ContactPhone = req.ContactPhone.Trim(),
            Status = TenantStatus.Active,
            CreatedBy = "ONBOARDING"
        };
        _context.Restaurants.Add(restaurant);

        var mainBranch = new Branch
        {
            RestaurantId = restaurant.Id,
            BranchName = string.IsNullOrWhiteSpace(req.BranchName) ? "Main Branch" : req.BranchName.Trim(),
            BranchCode = string.IsNullOrWhiteSpace(req.BranchCode) ? "MAIN" : req.BranchCode.Trim().ToUpperInvariant(),
            Address = "Headquarters",
            Phone = req.ContactPhone.Trim(),
            CreatedBy = "ONBOARDING"
        };
        _context.Branches.Add(mainBranch);

        var adminRole = new Role
        {
            RestaurantId = restaurant.Id,
            Name = "Restaurant Admin",
            Description = "Full administrative access to the restaurant tenant",
            IsSystemRole = true,
            CreatedBy = "ONBOARDING"
        };
        _context.Roles.Add(adminRole);

        // Assign core permissions to this role
        var permissions = await _context.Permissions.ToListAsync(cancellationToken);
        foreach (var perm in permissions)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RestaurantId = restaurant.Id,
                RoleId = adminRole.Id,
                PermissionId = perm.Id,
                CreatedBy = "ONBOARDING"
            });
        }

        var adminUser = new User
        {
            RestaurantId = restaurant.Id,
            BranchId = mainBranch.Id,
            Email = req.ContactEmail.Trim(),
            NormalizedEmail = normalizedEmail,
            FullName = req.AdminFullName.Trim(),
            PhoneNumber = req.ContactPhone.Trim(),
            PasswordHash = _passwordHasher.HashPassword(req.AdminPassword),
            Status = UserStatus.Active,
            CreatedBy = "ONBOARDING"
        };
        _context.Users.Add(adminUser);

        _context.UserRoles.Add(new UserRole
        {
            RestaurantId = restaurant.Id,
            UserId = adminUser.Id,
            RoleId = adminRole.Id,
            CreatedBy = "ONBOARDING"
        });

        _context.AuditLogs.Add(new AuditLog
        {
            RestaurantId = restaurant.Id,
            BranchId = mainBranch.Id,
            UserId = adminUser.Id,
            Action = "TENANT_REGISTERED",
            EntityName = nameof(Restaurant),
            EntityId = restaurant.Id.ToString(),
            IpAddress = cmd.IpAddress,
            UserAgent = cmd.UserAgent,
            CreatedBy = adminUser.Email
        });

        await _context.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(
            adminUser.Id,
            adminUser.RestaurantId,
            adminUser.BranchId,
            adminUser.Email,
            adminUser.FullName,
            adminUser.IsMfaEnabled,
            new List<string> { adminRole.Name },
            permissions.Select(p => p.Code).ToList()
        );

        return Result<UserDto>.Success(userDto);
    }
}
