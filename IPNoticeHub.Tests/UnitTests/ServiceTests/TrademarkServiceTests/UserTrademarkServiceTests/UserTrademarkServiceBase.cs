using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Services.TrademarkService.Implementations;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.TrademarkServiceTests.UserTrademarkServiceTests
{
    public class UserTrademarkServiceBase
    {
        protected IPNoticeHubDbContext testDbContext = null!;
        protected ApplicationUser user = null!;
        protected ITrademarkRepository tmRepository = null!;
        protected IUserTrademarkRepository userTmRepository = null!;
        protected TrademarkCollectionService service = null!;

        [SetUp]
        public void SetUp()
        {
            testDbContext = InMemoryDbContextFactory.CreateTestDbContext();

            user = InMemoryDbContextFactory.CreateApplicationUser();
            testDbContext.Users.Add(user);

            tmRepository = new TrademarkRepository(testDbContext);
            userTmRepository = new UserTrademarkRepository(testDbContext);

            service = new TrademarkCollectionService(tmRepository, userTmRepository);
        }

        [TearDown]
        public void TearDown()
        {
            testDbContext?.Dispose();
        }
    }
}
