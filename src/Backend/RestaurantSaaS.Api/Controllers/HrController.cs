using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.HR.Commands;
using RestaurantSaaS.Application.HR.Queries;

namespace RestaurantSaaS.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class HrController : ControllerBase
{
    private readonly IMediator _mediator;

    public HrController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("employees")]
    public async Task<ActionResult<Guid>> RegisterEmployee([FromBody] RegisterEmployeeCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(RegisterEmployee), new { id }, new { employeeId = id });
    }

    [HttpGet("branches/{branchId}/employees")]
    public async Task<ActionResult<List<EmployeeDto>>> GetBranchEmployees(Guid branchId)
    {
        var list = await _mediator.Send(new GetBranchEmployeesQuery(branchId));
        return Ok(list);
    }

    [HttpPost("attendance/clock")]
    public async Task<ActionResult<Guid>> ClockAttendance([FromBody] ClockAttendanceCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { attendanceId = id });
    }

    [HttpPost("payroll/calculate")]
    public async Task<ActionResult<Guid>> CalculatePayroll([FromBody] CalculateMonthlyPayrollCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { payrollRunId = id });
    }
}
