using IPNoticeHub.Application.Repositories.CopyrightRepository;
using IPNoticeHub.Application.Services.CopyrightService.Implementations;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Repositories.CopyrightRepository;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.CopyrightServiceTests
{
    public class CopyrightServiceBase
    {
        protected IPNoticeHubDbContext testDbContext = null!;
        protected ApplicationUser user = null!;
        protected ICopyrightRepository copyrightRepo = null!;
        protected IUserCopyrightRepository userCopyrightRepo = null!;
        protected CopyrightService service = null!;

        [SetUp]
        public async Task SetUp()
        {
            testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            user = InMemoryDbContextFactory.CreateApplicationUser();
            testDbContext.Users.Add(user);
            await testDbContext.SaveChangesAsync();

            copyrightRepo =
                new CopyrightRepository(testDbContext);

            userCopyrightRepo =
                new UserCopyrightRepository(testDbContext);

            service = new CopyrightService(
                copyrightRepo,
                userCopyrightRepo);
        }

        [TearDown]
        public void TearDown()
        {
            testDbContext?.Dispose();
        }
    }
}
