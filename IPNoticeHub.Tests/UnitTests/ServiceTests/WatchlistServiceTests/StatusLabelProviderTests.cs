using FluentAssertions;
using IPNoticeHub.Application.Services.WatchlistService.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.WatchlistTests
{
    public class StatusLabelProviderTests
    {
        [Test]
        public void GetStatusLabel_ReturnsLabel_WhenSourceIsUspto_AndStatusCodeExists()
        {
            using var temp = new TempFolder();

            var relativePath = Path.Combine(
                "Configurations",
                "uspto-status-codes.json");

            var fullPath = Path.Combine(temp.Path, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            File.WriteAllText(fullPath,
                """
                {
                  "codes": [
                    { "code": 100, "label": "Registered" },
                    { "code": 200, "label": "Abandoned" }
                  ]
                }
                """);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["StatusCodeCatalogs:USPTO"] = relativePath
                }).
                Build();

            var environment = new Mock<IHostEnvironment>();
            environment.SetupGet(e => e.ContentRootPath).Returns(temp.Path);

            var sut = new StatusLabelProvider(config, environment.Object);

            var result1 = sut.GetStatusLabel(
                "USPTO", 
                100);

            var result2 = sut.GetStatusLabel(
                "uspto", 
                200);

            result1.Should().Be("Registered");
            result2.Should().Be("Abandoned");
        }

        [Test]
        public void GetStatusLabel_ReturnsFallback_WhenSourceIsNotUspto()
        {
            using var temp = new TempFolder();

            var relativePath = "uspto.json";

            File.WriteAllText(Path.Combine(temp.Path, relativePath),
                """{ "codes": [ { "code": 100, "label": "Registered" } ] }""");

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["StatusCodeCatalogs:USPTO"] = relativePath
                }).
                Build();

            var env = new Mock<IHostEnvironment>();

            env.SetupGet(
                e => e.ContentRootPath).Returns(temp.Path);

            var sut = new StatusLabelProvider(
                config, 
                env.Object);

            var result = sut.GetStatusLabel("EUIPO", 100);

            result.Should().Be("Status 100");
        }

        [Test]
        public void GetStatusLabel_ReturnsFallback_WhenUsptoCodeIsMissing()
        {
            using var temp = new TempFolder();

            var relativePath = "uspto.json";

            File.WriteAllText(Path.Combine(temp.Path, relativePath),
                """{ "codes": [ { "code": 100, "label": "Registered" } ] }""");

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["StatusCodeCatalogs:USPTO"] = relativePath
                }).
                Build();

            var env = new Mock<IHostEnvironment>();

            env.SetupGet(
                e => e.ContentRootPath).
                Returns(temp.Path);

            var sut = new StatusLabelProvider(
                config, 
                env.Object);

            var result = sut.GetStatusLabel(
                "USPTO", 
                999);

            result.Should().Be("Status 999");
        }

        [Test]
        public void GetStatusLabel_UsesFallbackLabel_WhenJsonLabelIsNull()
        {
            using var temp = new TempFolder();

            var relativePath = "uspto.json";
            File.WriteAllText(Path.Combine(temp.Path, relativePath),
                """
                {
                  "codes": [
                    { "code": 321, "label": null }
                  ]
                }
                """);

            var config = new ConfigurationBuilder().
                AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["StatusCodeCatalogs:USPTO"] = relativePath
                }).
                Build();

            var env = new Mock<IHostEnvironment>();

            env.SetupGet(
                e => e.ContentRootPath).Returns(temp.Path);

            var sut = new StatusLabelProvider(
                config, 
                env.Object);

            var result = sut.GetStatusLabel(
                "USPTO", 
                321);

            result.Should().Be("Status 321");
        }

        private sealed class TempFolder : IDisposable
        {
            public string Path { get; } =
                System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(), 
                    "ipnoticehub-tests-" + Guid.NewGuid());

            public TempFolder() => Directory.CreateDirectory(Path);

            public void Dispose()
            {
                try { Directory.Delete(Path, recursive: true); }
                catch {}
            }
        }
    }
}
