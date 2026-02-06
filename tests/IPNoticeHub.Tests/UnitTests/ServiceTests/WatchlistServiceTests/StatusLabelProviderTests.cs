using FluentAssertions;
using IPNoticeHub.Application.Services.WatchlistService.Implementations;
using IPNoticeHub.Tests.UnitTests.ServiceTests.WatchlistServiceTests;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.WatchlistTests
{
    public class StatusLabelProviderTests : StatusLabelProviderBase
    {
        [Test]
        public void GetStatusLabel_ReturnsLabel_WhenSourceIsUspto_AndStatusCodeExists()
        {
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

            var config = new ConfigurationBuilder().
                AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["StatusCodeCatalogs:USPTO"] = relativePath
                }).
                Build();

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
            var relativePath = "uspto.json";

            File.WriteAllText(Path.Combine(temp.Path, relativePath),
                """{ "codes": [ { "code": 100, "label": "Registered" } ] }""");

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["StatusCodeCatalogs:USPTO"] = relativePath
                }).
                Build();

            environment.SetupGet(
                e => e.ContentRootPath).Returns(temp.Path);

            var labelProvider = new StatusLabelProvider(
                config, 
                environment.Object);

            var result = labelProvider.GetStatusLabel("EUIPO", 100);

            result.Should().Be("Status 100");
        }

        [Test]
        public void GetStatusLabel_ReturnsFallback_WhenUsptoCodeIsMissing()
        {
            var relativePath = "uspto.json";

            File.WriteAllText(Path.Combine(temp.Path, relativePath),
                """{ "codes": [ { "code": 100, "label": "Registered" } ] }""");

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["StatusCodeCatalogs:USPTO"] = relativePath
                }).
                Build();

            environment.SetupGet(
                e => e.ContentRootPath).
                Returns(temp.Path);

            var labelProvider = new StatusLabelProvider(
                config, 
                environment.Object);

            var result = labelProvider.GetStatusLabel(
                "USPTO", 
                999);

            result.Should().Be("Status 999");
        }

        [Test]
        public void GetStatusLabel_UsesFallbackLabel_WhenJsonLabelIsNull()
        {
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

            environment.SetupGet(
                e => e.ContentRootPath).Returns(temp.Path);

            var labelProvider = new StatusLabelProvider(
                config, 
                environment.Object);

            var result = labelProvider.GetStatusLabel(
                "USPTO", 
                321);

            result.Should().Be("Status 321");
        }
    }
}
