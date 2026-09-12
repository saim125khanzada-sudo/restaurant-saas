namespace RestaurantSaaS.Application.DTOs;

public record LoginRequest(string Email, string Password, string? MfaCode, string DeviceFingerprint);
public record LoginResponse(
    bool RequiresMfa,
    string? AccessToken,
    string? RefreshToken,
    DateTimeOffset? ExpiresAt,
    UserDto? User
);

public record RefreshTokenRequest(string RefreshToken, string DeviceFingerprint);

public record RegisterTenantAdminRequest(
    string RestaurantName,
    string RestaurantCode,
    string ContactEmail,
    string ContactPhone,
    string AdminFullName,
    string AdminPassword,
    string BranchName,
    string BranchCode
);

public record MfaSetupResponse(string Secret, string QrCodeUri);
public record MfaVerifyRequest(string Code);

public record UserDto(
    Guid Id,
    Guid RestaurantId,
    Guid? BranchId,
    string Email,
    string FullName,
    bool IsMfaEnabled,
    List<string> Roles,
    List<string> Permissions
);
