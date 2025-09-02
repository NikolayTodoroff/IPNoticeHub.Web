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
    public static class InMemoryDbFactory
    {
        /// <summary>
        /// Creates an in-memory database context for testing purposes.
        /// </summary>
        public static IPNoticeHubDbContext Create(string? dbName = null)
        {
            DbContextOptions<IPNoticeHubDbContext>? options = new DbContextOptionsBuilder<IPNoticeHubDbContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            IPNoticeHubDbContext? ctx = new IPNoticeHubDbContext(options);
            ctx.Database.EnsureCreated();
            return ctx;
        }

        /// <summary>
        /// Creates a TrademarkEntity and its associated classes for testing purposes.
        /// </summary>
        public static (TrademarkEntity trademarkEntity, List<TrademarkClassAssignment> trademarkClasses) CreateTrademark(
            int id,
            string wordmark,
            string owner,
            string? regNumber,
            TrademarkStatusCategory status = TrademarkStatusCategory.Pending,
            DataProvider source = DataProvider.USPTO,
            params int[] classNumbers)
        {
            TrademarkEntity? trademarkEntity = new TrademarkEntity
            {
                Id = id,
                PublicId = Guid.NewGuid(),
                Wordmark = wordmark,
                Owner = owner,
                RegistrationNumber = regNumber,
                GoodsAndServices = "Dummy goods and services",
                StatusCategory = status,
                Source = source,
                FilingDate = DateTime.UtcNow.Date,
            };

            List<TrademarkClassAssignment>? trademarksList = new List<TrademarkClassAssignment>();
            foreach (var classNumber in classNumbers)
            {
                trademarksList.Add(new TrademarkClassAssignment
                {
                    ClassNumber = classNumber,
                    TrademarkRegistration = trademarkEntity
                });
            }

            trademarkEntity.Classes = trademarksList;
            return (trademarkEntity, trademarksList);
        }

        /// <summary>
        /// Creates and returns a test ApplicationUser instance with default test data.
        /// </summary>
        public static ApplicationUser CreateApplicationUser(string id = "user-1")
        {
            return new ApplicationUser
            {
                Id = id,
                UserName = "tester@example.com",
                Email = "tester@example.com"
            };
        }
    }
}

