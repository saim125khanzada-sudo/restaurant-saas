using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.HR.Queries;

public record EmployeeDto(
    Guid Id,
    string EmployeeCode,
    string FullName,
    string Designation,
    string? Phone,
    string? Email,
    EmploymentStatus Status,
    decimal BaseMonthlySalary
);

public record GetBranchEmployeesQuery(Guid BranchId) : IRequest<List<EmployeeDto>>;

public class GetBranchEmployeesQueryHandler : IRequestHandler<GetBranchEmployeesQuery, List<EmployeeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBranchEmployeesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeeDto>> Handle(GetBranchEmployeesQuery request, CancellationToken cancellationToken)
    {
        var employees = await _context.Employees
            .Where(e => e.BranchId == request.BranchId)
            .OrderBy(e => e.FullName)
            .ToListAsync(cancellationToken);

        return employees.Select(e => new EmployeeDto(
            e.Id,
            e.EmployeeCode,
            e.FullName,
            e.Designation,
            e.Phone,
            e.Email,
            e.Status,
            e.BaseMonthlySalary
        )).ToList();
    }
}
