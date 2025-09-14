using FluentAssertions;
using IPNoticeHub.Tests.IntegrationTests.TestUtilities;
using NUnit.Framework;
using System.Net;

namespace IPNoticeHub.Tests.IntegrationTests.CopyrightIntegrationTests
{
    /// <summary>
    /// GET /Copyrights/Create should render the empty creation form.
    /// Verifies the endpoint responds with 200 OK under normal conditions.
    /// </summary>
    public class CopyrightsCreateIntTests
    {
        private TestWebAppFactory appFactory = null!;

        [SetUp]
        public void SetUp() => appFactory = new TestWebAppFactory();

        [TearDown]
        public void TearDown() => appFactory.Dispose();
       
        [Test]
        public async Task Get_Create_Returns200()
        {
            var client = appFactory.CreateClientAs("u1");

            var response = await client.GetAsync("/Copyrights/Create");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
