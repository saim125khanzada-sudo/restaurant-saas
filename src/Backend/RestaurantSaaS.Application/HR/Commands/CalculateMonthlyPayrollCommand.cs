using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.HR.Commands;

public record CalculateMonthlyPayrollCommand(
    Guid BranchId,
    int Month,
    int Year
) : IRequest<Guid>;

public class CalculateMonthlyPayrollCommandValidator : AbstractValidator<CalculateMonthlyPayrollCommand>
{
    public CalculateMonthlyPayrollCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).GreaterThanOrEqualTo(2024);
    }
}

public class CalculateMonthlyPayrollCommandHandler : IRequestHandler<CalculateMonthlyPayrollCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public CalculateMonthlyPayrollCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Guid> Handle(CalculateMonthlyPayrollCommand request, CancellationToken cancellationToken)
    {
        var restaurantId = _tenantService.RestaurantId ?? Guid.Empty;

        // Fetch all active employees for this branch
        var employees = await _context.Employees
            .Where(e => e.BranchId == request.BranchId && e.Status == EmploymentStatus.Active)
            .ToListAsync(cancellationToken);

        var daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
        var monthStart = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var payrollRun = new PayrollRun
        {
            RestaurantId = restaurantId,
            BranchId = request.BranchId,
            Month = request.Month,
            Year = request.Year,
            Status = PayrollStatus.Calculated,
            ProcessedAt = DateTime.UtcNow
        };

        decimal totalGross = 0;
        decimal totalDeductions = 0;
        decimal totalNet = 0;

        foreach (var emp in employees)
        {
            // Calculate attendance & overtime in the period
            var attendanceList = await _context.AttendanceRecords
                .Where(a => a.EmployeeId == emp.Id && a.WorkDate >= monthStart && a.WorkDate < monthEnd)
                .ToListAsync(cancellationToken);

            var daysPresent = attendanceList.Count(a => a.ClockOutTime.HasValue);
            var daysAbsent = Math.Max(0, 26 - daysPresent); // standard 26-day working month
            var totalOvertimeHours = attendanceList.Sum(a => a.OvertimeHours);
            var overtimePay = totalOvertimeHours * emp.HourlyOvertimeRate;

            // Check staff advances / loans
            var activeAdvances = await _context.StaffAdvances
                .Where(a => a.EmployeeId == emp.Id && !a.IsFullyRepaid && a.RemainingBalance > 0)
                .ToListAsync(cancellationToken);

            decimal advanceDeduction = 0;
            foreach (var adv in activeAdvances)
            {
                var deduction = Math.Min(adv.MonthlyDeductionAmount, adv.RemainingBalance);
                advanceDeduction += deduction;
                adv.RemainingBalance -= deduction;
                if (adv.RemainingBalance <= 0)
                {
                    adv.IsFullyRepaid = true;
                }
            }

            // Prorated base salary if absent
            decimal dailyRate = emp.BaseMonthlySalary / 26m;
            decimal earnedBase = Math.Min(emp.BaseMonthlySalary, daysPresent * dailyRate);
            decimal gross = earnedBase + overtimePay;
            decimal net = Math.Max(0, gross - advanceDeduction);

            var detail = new PayrollDetail
            {
                RestaurantId = restaurantId,
                EmployeeId = emp.Id,
                BaseSalary = earnedBase,
                DaysPresent = daysPresent,
                DaysAbsent = daysAbsent,
                OvertimePay = overtimePay,
                AdvanceDeduction = advanceDeduction,
                OtherDeductions = 0,
                NetSalaryPayable = net
            };

            payrollRun.Details.Add(detail);

            totalGross += gross;
            totalDeductions += advanceDeduction;
            totalNet += net;
        }

        payrollRun.TotalGrossPay = totalGross;
        payrollRun.TotalDeductions = totalDeductions;
        payrollRun.TotalNetPay = totalNet;

        _context.PayrollRuns.Add(payrollRun);
        await _context.SaveChangesAsync(cancellationToken);

        return payrollRun.Id;
    }
}
