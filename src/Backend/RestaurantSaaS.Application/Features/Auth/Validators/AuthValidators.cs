using FluentValidation;
using RestaurantSaaS.Application.Features.Auth.Commands;

namespace RestaurantSaaS.Application.Features.Auth.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("A valid email address is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
        RuleFor(x => x.DeviceFingerprint).NotEmpty().WithMessage("Device fingerprint is required.");
    }
}

public class RegisterTenantCommandValidator : AbstractValidator<RegisterTenantCommand>
{
    public RegisterTenantCommandValidator()
    {
        RuleFor(x => x.Request.RestaurantName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.RestaurantCode).NotEmpty().MinimumLength(3).MaximumLength(20);
        RuleFor(x => x.Request.ContactEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.ContactPhone).NotEmpty().MinimumLength(7).MaximumLength(20);
        RuleFor(x => x.Request.AdminFullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.AdminPassword).NotEmpty().MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
    }
}
