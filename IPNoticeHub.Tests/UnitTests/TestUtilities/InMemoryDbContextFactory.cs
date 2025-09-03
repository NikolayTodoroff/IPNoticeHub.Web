using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Common.EnumConstants;
using System.Reflection;

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
        public static IPNoticeHubDbContext CreateTestDbContext(string? dbContextName = null, bool clearSeed = true)
        {
            DbContextOptions<IPNoticeHubDbContext>? options = new DbContextOptionsBuilder<IPNoticeHubDbContext>()
                .UseInMemoryDatabase(dbContextName ?? Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            var testDbContext = new IPNoticeHubDbContext(options);
            testDbContext.Database.EnsureCreated();

            if (clearSeed)
                ClearAllData(testDbContext);

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

        /// <summary>
        /// Removes all rows from all mapped DbSets in the provided DbContext.
        /// Ensures that tests start with a clean slate, even if the model includes HasData seeding.
        /// </summary>
        private static void ClearAllData(IPNoticeHubDbContext testDbContext)
        {
            // Clear the change tracker to avoid tracking issues during data removal
            testDbContext.ChangeTracker.Clear();

            // Retrieve the generic DbContext.Set<TEntity>() method for accessing DbSets dynamically
            MethodInfo? setMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!;

            // Iterate through all entity types in the model, excluding owned types
            foreach (var entityType in testDbContext.Model.GetEntityTypes().Where(t => !t.IsOwned()))
            {
                var clrType = entityType.ClrType; // Get the CLR type of the entity
                if (clrType == null) continue;

                // Dynamically invoke Set<TEntity>() to get the DbSet for the entity type
                var genericSet = setMethod.MakeGenericMethod(clrType).Invoke(testDbContext, null)!;

                // Treat the dynamically retrieved DbSet as an IQueryable for enumeration
                var queryable = (IQueryable)genericSet;

                // Convert the IQueryable to a list of objects representing all rows in the DbSet
                var entities = queryable.Cast<object>().ToList();

                // Remove all entities from the DbSet if any exist
                if (entities.Count > 0)
                    testDbContext.RemoveRange(entities);
            }

            testDbContext.SaveChanges();
            testDbContext.ChangeTracker.Clear();
        }
    }
}

