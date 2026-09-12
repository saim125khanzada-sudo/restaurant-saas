using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.Domain.Exceptions;

namespace RestaurantSaaS.Application.HR.Commands;

public record ClockAttendanceCommand(
    Guid EmployeeId,
    DateTime PunchTime,
    AttendanceSource Source,
    string? DeviceIdentifier
) : IRequest<Guid>;

public class ClockAttendanceCommandValidator : AbstractValidator<ClockAttendanceCommand>
{
    public ClockAttendanceCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}

public class ClockAttendanceCommandHandler : IRequestHandler<ClockAttendanceCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public ClockAttendanceCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Guid> Handle(ClockAttendanceCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken)
            ?? throw new NotFoundException($"Employee {request.EmployeeId} not found.");

        var today = request.PunchTime.Date;

        // Check if there's already an open attendance record for today
        var existing = await _context.AttendanceRecords
            .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.WorkDate == today, cancellationToken);

        if (existing == null)
        {
            // Clock In
            var record = new AttendanceRecord
            {
                RestaurantId = _tenantService.RestaurantId ?? employee.RestaurantId,
                BranchId = employee.BranchId,
                EmployeeId = employee.Id,
                WorkDate = today,
                ClockInTime = request.PunchTime,
                Source = request.Source,
                DeviceIdentifier = request.DeviceIdentifier
            };
            _context.AttendanceRecords.Add(record);
            await _context.SaveChangesAsync(cancellationToken);
            return record.Id;
        }
        else
        {
            // Clock Out
            existing.ClockOutTime = request.PunchTime;
            var duration = request.PunchTime - existing.ClockInTime;
            var hours = (decimal)duration.TotalHours;
            existing.TotalHoursWorked = Math.Round(hours, 2);

            // Standard shift = 8 hours; remainder is overtime
            if (existing.TotalHoursWorked > 8.0m)
            {
                existing.OvertimeHours = existing.TotalHoursWorked - 8.0m;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }
    }
}
