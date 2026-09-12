using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.HR.Commands;

public record RegisterEmployeeCommand(
    Guid BranchId,
    string EmployeeCode,
    string FullName,
    string Designation,
    string? Phone,
    string? Email,
    string? NationalId,
    string? BiometricUserId,
    DateTime DateOfJoining,
    decimal BaseMonthlySalary,
    decimal HourlyOvertimeRate
) : IRequest<Guid>;

public class RegisterEmployeeCommandValidator : AbstractValidator<RegisterEmployeeCommand>
{
    public RegisterEmployeeCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Designation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BaseMonthlySalary).GreaterThan(0);
        RuleFor(x => x.HourlyOvertimeRate).GreaterThanOrEqualTo(0);
    }
}

public class RegisterEmployeeCommandHandler : IRequestHandler<RegisterEmployeeCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public RegisterEmployeeCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Guid> Handle(RegisterEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = new Employee
        {
            RestaurantId = _tenantService.RestaurantId ?? Guid.Empty,
            BranchId = request.BranchId,
            EmployeeCode = request.EmployeeCode,
            FullName = request.FullName,
            Designation = request.Designation,
            Phone = request.Phone,
            Email = request.Email,
            NationalId = request.NationalId,
            BiometricUserId = request.BiometricUserId,
            DateOfJoining = request.DateOfJoining,
            Status = EmploymentStatus.Active,
            BaseMonthlySalary = request.BaseMonthlySalary,
            HourlyOvertimeRate = request.HourlyOvertimeRate
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);

        return employee.Id;
    }
}
