namespace RestaurantSaaS.Domain.Enums;

public enum EmploymentStatus
{
    Active = 1,
    Probation = 2,
    Suspended = 3,
    Terminated = 4,
    Resigned = 5
}

public enum AttendanceSource
{
    ManualPosPin = 1,
    BiometricHardware = 2,
    MobileGps = 3,
    WebAdmin = 4
}

public enum PayrollStatus
{
    Draft = 1,
    Calculated = 2,
    Approved = 3,
    Disbursed = 4,
    Cancelled = 5
}
