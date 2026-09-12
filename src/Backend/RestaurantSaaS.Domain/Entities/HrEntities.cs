using System;
using System.Collections.Generic;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.SharedKernel.Common;
using RestaurantSaaS.SharedKernel.Interfaces;

namespace RestaurantSaaS.Domain.Entities;

public class Employee : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? UserId { get; set; } // Optional link to staff User account
    public string EmployeeCode { get; set; } = string.Empty; // e.g. "EMP-001"
    public string FullName { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty; // e.g. "Head Chef", "Captain Waiter"
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? NationalId { get; set; } // CNIC / SSN
    public string? BiometricUserId { get; set; } // Hardware enrollment ID
    public DateTime DateOfJoining { get; set; }
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;

    public decimal BaseMonthlySalary { get; set; }
    public decimal HourlyOvertimeRate { get; set; }

    // Navigation
    public Branch? Branch { get; set; }
    public User? User { get; set; }
}

public class AttendanceRecord : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime WorkDate { get; set; }
    public DateTime ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public decimal TotalHoursWorked { get; set; }
    public decimal OvertimeHours { get; set; }
    public AttendanceSource Source { get; set; } = AttendanceSource.ManualPosPin;
    public string? DeviceIdentifier { get; set; }

    // Navigation
    public Employee? Employee { get; set; }
}

public class StaffAdvance : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime IssuedDate { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal MonthlyDeductionAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public bool IsFullyRepaid { get; set; } = false;
    public string? Purpose { get; set; }

    // Navigation
    public Employee? Employee { get; set; }
}

public class PayrollRun : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;

    public decimal TotalGrossPay { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetPay { get; set; }

    public DateTime? ProcessedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }

    // Navigation
    public List<PayrollDetail> Details { get; set; } = new();
}

public class PayrollDetail : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid PayrollRunId { get; set; }
    public Guid EmployeeId { get; set; }

    public decimal BaseSalary { get; set; }
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal NetSalaryPayable { get; set; }

    // Navigation
    public Employee? Employee { get; set; }
}
