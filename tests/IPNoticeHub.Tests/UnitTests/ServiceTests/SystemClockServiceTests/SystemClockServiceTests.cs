using NUnit.Framework;
using FluentAssertions;
using IPNoticeHub.Infrastructure.Persistence.Services.SystemClockService.Implementation;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.SystemClockServiceTests
{
    public class SystemClockServiceTests
    {
        [Test]
        public void UtcNow_ReturnsUtcDateTime()
        {
            var service = new SystemClockService();
            var result = service.UtcNow;

            result.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Test]
        public void UtcNow_ReturnsCurrentTime()
        {
            var service = new SystemClockService();

            var before = DateTime.UtcNow;

            var result = service.UtcNow;

            var after = DateTime.UtcNow;

            result.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        }

        [Test]
        public void UtcNow_MultipleCallsReturnSimilarTimes()
        {
            var service = new SystemClockService();

            var first = service.UtcNow;
            var second = service.UtcNow;

            var difference = (second - first).TotalMilliseconds;
            difference.Should().BeInRange(0, 100);
        }
    }
}
