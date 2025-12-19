using IPNoticeHub.Application.Services.SystemClockService.Abstractions;

namespace IPNoticeHub.Infrastructure.Services.SystemClockService.Implementation;

public sealed class SystemClockService : ISystemClockService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
