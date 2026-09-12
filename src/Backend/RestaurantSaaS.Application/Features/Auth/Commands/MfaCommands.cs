using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.SharedKernel.Common;

namespace RestaurantSaaS.Application.Features.Auth.Commands;

public record SetupMfaCommand(Guid UserId) : IRequest<Result<MfaSetupResponse>>;

public class SetupMfaCommandHandler : IRequestHandler<SetupMfaCommand, Result<MfaSetupResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMfaService _mfaService;

    public SetupMfaCommandHandler(IApplicationDbContext context, IMfaService mfaService)
    {
        _context = context;
        _mfaService = mfaService;
    }

    public async Task<Result<MfaSetupResponse>> Handle(SetupMfaCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null)
            return Result<MfaSetupResponse>.Failure("User not found.");

        var secret = _mfaService.GenerateSecret();
        user.MfaSecret = secret;
        await _context.SaveChangesAsync(cancellationToken);

        var qrCodeUri = _mfaService.GenerateQrCodeUri(user.Email, secret);
        return Result<MfaSetupResponse>.Success(new MfaSetupResponse(secret, qrCodeUri));
    }
}

public record VerifyMfaCommand(Guid UserId, string Code) : IRequest<Result<bool>>;

public class VerifyMfaCommandHandler : IRequestHandler<VerifyMfaCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMfaService _mfaService;

    public VerifyMfaCommandHandler(IApplicationDbContext context, IMfaService mfaService)
    {
        _context = context;
        _mfaService = mfaService;
    }

    public async Task<Result<bool>> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null || string.IsNullOrWhiteSpace(user.MfaSecret))
            return Result<bool>.Failure("MFA setup is not initialized for this user.");

        if (!_mfaService.VerifyTotp(user.MfaSecret, request.Code))
            return Result<bool>.Failure("Invalid verification code.");

        user.IsMfaEnabled = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
