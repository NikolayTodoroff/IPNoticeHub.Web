using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Tests.IntegrationTests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Net;

namespace IPNoticeHub.Tests.IntegrationTests.TrademarkIntegrationTests
{
    public class TrademarkMyCollectionIntegrationTests
    {
        private TestWebAppFactory appFactory = null!;

        [SetUp]
        public void SetUp() => appFactory = new TestWebAppFactory();

        [TearDown]
        public void TearDown() => appFactory.Dispose();

        [Test]
        public async Task Get_MyCollection_Unauthenticated_Returns401()
        {
            var client = appFactory.CreateClient(new()
            {
                AllowAutoRedirect = false
            });

            var response = await client.GetAsync("/Trademarks/MyCollection");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
