using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.WatchlistServiceTests
{
    public class StatusLabelProviderBase
    {
        protected Mock<IHostEnvironment> environment = null!;
        protected TempFolder temp = null!;

        [SetUp]
        public void SetUp()
        {
            environment = new Mock<IHostEnvironment>(MockBehavior.Strict);
            temp = new TempFolder();
        }
        public sealed class TempFolder : IDisposable
        {
            public string Path { get; } =
                System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    "ipnoticehub-tests-" + Guid.NewGuid());

            public TempFolder() => Directory.CreateDirectory(Path);

            public void Dispose()
            {
                try { Directory.Delete(Path, recursive: true); }
                catch { }
            }
        }
    }
}
