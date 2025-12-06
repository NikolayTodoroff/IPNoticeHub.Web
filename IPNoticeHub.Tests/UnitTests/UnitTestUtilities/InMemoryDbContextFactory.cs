using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Data.Entities.CopyrightRegistration;
using IPNoticeHub.Data.Entities.Identity;
using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Tests.TestUtilities
{
    /// <summary>
    /// Provides utility methods for creating in-memory database contexts and test data for unit tests.
    /// </summary>
    public static class InMemoryDbContextFactory
    {
        /// <summary>
        /// Creates a clean in-memory database context for testing purposes.
        /// </summary>
        public static IPNoticeHubDbContext CreateTestDbContext(string? dbContextName = null)
        {
            DbContextOptions<IPNoticeHubDbContext>? options = new DbContextOptionsBuilder<IPNoticeHubDbContext>()
                .UseInMemoryDatabase(dbContextName ?? Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            var testDbContext = new IPNoticeHubDbContext(options);
            testDbContext.Database.EnsureCreated();

            return testDbContext;
        }

        /// <summary>
        /// Creates a TrademarkEntity and its associated classes for testing purposes.
        /// </summary>
        public static (TrademarkEntity trademarkEntity, List<TrademarkClassAssignment> trademarkClasses) CreateTrademark(
            string wordmark, string owner, string goodsAndServices,string sourceId, string statusDetail, 
            string? regNumber,TrademarkStatusCategory status = TrademarkStatusCategory.Pending,
            DataProvider source = DataProvider.USPTO, DateTime? filingDate = null, params int[] classNumbers)
        {
            TrademarkEntity? trademarkEntity = new TrademarkEntity
            {               
                PublicId = Guid.NewGuid(),
                Wordmark = wordmark,
                Owner = owner,
                GoodsAndServices = goodsAndServices,
                SourceId = sourceId,
                StatusDetail = statusDetail,
                RegistrationNumber = regNumber,
                StatusCategory = status,
                Source = source,
                FilingDate = filingDate ?? DateTime.UtcNow.Date
            };

            List<TrademarkClassAssignment>? trademarkClassesList = new List<TrademarkClassAssignment>();

            foreach (int classNumber in classNumbers ?? Array.Empty<int>())
            {
                trademarkClassesList.Add(new TrademarkClassAssignment
                {
                    ClassNumber = classNumber,
                    TrademarkRegistration = trademarkEntity
                });
            }

            trademarkEntity.Classes = trademarkClassesList;
            return (trademarkEntity, trademarkClassesList);
        }

        /// <summary>
        /// Creates a CopyrightRegistration entity for testing.
        /// Only sets the core fields you use in tests; everything else stays default/null.
        /// </summary>
        public static CopyrightEntity CreateCopyright(
            string registrationNumber = "TX-0000000",
            string title = "Test Title",
            string typeOfWork = "Literary Work",
            string owner = "Test Owner",
            int? yearOfCreation = null,
            DateTime? dateOfPublication = null,
            string? nationOfFirstPublication = null)
        {
            return new CopyrightEntity
            {
                RegistrationNumber = registrationNumber ?? string.Empty,
                TypeOfWork = typeOfWork ?? string.Empty,
                Title = title ?? string.Empty,
                YearOfCreation = yearOfCreation,
                DateOfPublication = dateOfPublication,
                Owner = owner ?? string.Empty,
                NationOfFirstPublication = nationOfFirstPublication
            };
        }

        /// <summary>
        /// Creates and returns a test ApplicationUser instance with default test data.
        /// </summary>
        public static ApplicationUser CreateApplicationUser(string id = "user-1")
        {
            return new ApplicationUser
            {
                Id = id,
                UserName = "testerUserName",
                Email = "tester@example.com"
            };
        }
    }
}

