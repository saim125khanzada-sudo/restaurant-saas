using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.HR.Services;

public record BiometricPunchLog(
    string BiometricUserId,
    DateTime PunchTime,
    string DeviceId,
    string PunchType // ClockIn, ClockOut
);

public interface IBiometricAttendanceAdapter
{
    Task<List<BiometricPunchLog>> FetchNewPunchesAsync(string deviceIp, int port, CancellationToken cancellationToken = default);
}

public class MockBiometricAttendanceAdapter : IBiometricAttendanceAdapter
{
    public Task<List<BiometricPunchLog>> FetchNewPunchesAsync(string deviceIp, int port, CancellationToken cancellationToken = default)
    {
        // Production adapter implements TCP socket push SDK (e.g. ZKTeco / standalone pull)
        var list = new List<BiometricPunchLog>();
        return Task.FromResult(list);
    }
}
