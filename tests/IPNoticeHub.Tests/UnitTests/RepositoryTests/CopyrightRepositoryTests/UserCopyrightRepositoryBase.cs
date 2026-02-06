using IPNoticeHub.Application.Repositories.CopyrightRepository;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Repositories.CopyrightRepository;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.CopyrightRepositoryTests
{
    public class UserCopyrightRepositoryBase
    {
        protected IPNoticeHubDbContext testDbContext = null!;
        protected IUserCopyrightRepository repository = null!;
        protected ApplicationUser user = null!;

        [SetUp]
        public void SetUp()
        {
            testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            repository =
                new UserCopyrightRepository(testDbContext);

            user = new ApplicationUser
            {
                Id = "1234556",
                UserName = "TestUser",
                Email = "user@test.com"
            };
        }
    }
}
