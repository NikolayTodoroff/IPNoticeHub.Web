using IPNoticeHub.Application.Services.TrademarkSearchService.Abstractions;
using Moq;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.HomeControllerTests
{
    public class HomeControllerBase
    {
        protected Mock<ITrademarkSearchQueryService> service = null!;

        [SetUp]
        public void SetUp()
        {
            service = new Mock<ITrademarkSearchQueryService>(MockBehavior.Strict);
        }
    }
}
