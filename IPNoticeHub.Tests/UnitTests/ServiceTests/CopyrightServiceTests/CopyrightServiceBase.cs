using IPNoticeHub.Application.Repositories.CopyrightRepository;
using IPNoticeHub.Application.Services.CopyrightService.Implementations;
using IPNoticeHub.Domain.Entities.Copyrights;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Repositories.CopyrightRepository;
using IPNoticeHub.Shared.Enums;
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

        protected CopyrightEntity cpEntity1 = null!;
        protected CopyrightEntity cpEntity2 = null!;
        protected CopyrightEntity cpEntity3 = null!;

        [SetUp]
        public void SetUp()
        {
            testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            user = InMemoryDbContextFactory.CreateApplicationUser();
            testDbContext.Users.Add(user);

            var testCopyrights =
               TestCopyrightData.CreateTestCopyrights(
                   out cpEntity1,
                   out cpEntity2,
                   out cpEntity3);

            testDbContext.CopyrightRegistrations.AddRange(testCopyrights);
            testDbContext.SaveChangesAsync();

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

        protected static class TestCopyrightData
        {
            public static CopyrightEntity[] CreateTestCopyrights(
                out CopyrightEntity cpEntity1,
                out CopyrightEntity cpEntity2,
                out CopyrightEntity cpEntity3)
            {
                cpEntity1 = InMemoryDbContextFactory.CreateCopyright(
                    registrationNumber: "1234567",
                    title: "Title 1",
                    typeOfWork: CopyrightWorkType.Literary.ToString(),
                    owner: "Owner 1",
                    yearOfCreation: 2020,
                    dateOfPublication: new DateTime(2020, 1, 1),
                    nationOfFirstPublication: "US");

                cpEntity2 = InMemoryDbContextFactory.CreateCopyright(
                    registrationNumber: "7654321",
                    title: "Title 2",
                    typeOfWork: CopyrightWorkType.Audiovisual.ToString(),
                    owner: "Owner 2",
                    yearOfCreation: 2022,
                    dateOfPublication: new DateTime(2022, 2, 5),
                    nationOfFirstPublication: "France");


                cpEntity3 = InMemoryDbContextFactory.CreateCopyright(
                    registrationNumber: "1423563",
                    title: "Title 3",
                    typeOfWork: CopyrightWorkType.VisualArts.ToString(),
                    owner: "Owner 3",
                    yearOfCreation: 2025,
                    dateOfPublication: new DateTime(2025, 6, 8),
                    nationOfFirstPublication: "US");

                return new[] { cpEntity1, cpEntity2, cpEntity3 };
            }
        }
    }
}
