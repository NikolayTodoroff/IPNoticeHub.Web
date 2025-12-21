using IPNoticeHub.Application.Services.SystemClockService.Abstractions;

namespace IPNoticeHub.Tests.UnitTests.UnitTestFactories
{
    public sealed class TestClock : ISystemClockService
    {
        public DateTime UtcNow { get; set; } =
        new DateTime(2030, 01, 01,
            0, 0, 0, DateTimeKind.Utc);
    }
}
