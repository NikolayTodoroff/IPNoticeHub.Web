namespace IPNoticeHub.Application.Services.SystemClockService.Abstractions
{
    public interface ISystemClockService
    {
        DateTime UtcNow { get; }
    }
}
