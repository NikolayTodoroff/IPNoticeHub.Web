using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Tests.TestUtilities
{
    /// <summary>
    /// Provides utility methods for creating in-memory database contexts and test data for unit tests.
    /// </summary>
    public static class TestDbContextFactory
    {
        /// <summary>
        /// Creates an in-memory database context for testing purposes.
        /// </summary>
        public static IPNoticeHubDbContext CreateTestDbContext(string? dbName = null)
        {
            DbContextOptions<IPNoticeHubDbContext>? options = new DbContextOptionsBuilder<IPNoticeHubDbContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            IPNoticeHubDbContext? testDbContext = new IPNoticeHubDbContext(options);
            testDbContext.Database.EnsureCreated();
            return testDbContext;
        }

        /// <summary>
        /// Creates a TrademarkEntity and its associated classes for testing purposes.
        /// </summary>
        public static (TrademarkEntity trademarkEntity, List<TrademarkClassAssignment> trademarkClasses) CreateTrademark(
            string wordmark, string owner, string? regNumber, TrademarkStatusCategory status = TrademarkStatusCategory.Pending,
            DataProvider source = DataProvider.USPTO, DateTime? filingDate = null, params int[] classNumbers)
        {
            TrademarkEntity? trademarkEntity = new TrademarkEntity
            {               
                PublicId = Guid.NewGuid(),
                Wordmark = wordmark,
                Owner = owner,
                RegistrationNumber = regNumber,
                GoodsAndServices = "Test goods and services",
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

