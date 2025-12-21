using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Domain.Entities.Copyrights;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Identity;

namespace IPNoticeHub.Tests.UnitTests.UnitTestFactories
{
    public static class InMemoryDbContextFactory
    {
        public static IPNoticeHubDbContext CreateTestDbContext(string? dbContextName = null)
        {
            DbContextOptions<IPNoticeHubDbContext>? options = 
                new DbContextOptionsBuilder<IPNoticeHubDbContext>()
                .UseInMemoryDatabase(dbContextName ?? Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            var testDbContext = new IPNoticeHubDbContext(options);
            testDbContext.Database.EnsureCreated();

            return testDbContext;
        }

        public static (TrademarkEntity trademarkEntity, List<TrademarkClassAssignment> trademarkClasses) 
            CreateTrademark(
            string wordmark, 
            string owner, 
            string goodsAndServices,
            string sourceId, 
            string statusDetail, 
            string? regNumber,
            TrademarkStatusCategory status = TrademarkStatusCategory.Pending,
            DataProvider source = DataProvider.USPTO, 
            DateTime? filingDate = null, 
            params int[] classNumbers)
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

            List<TrademarkClassAssignment>? trademarkClassesList = 
                new List<TrademarkClassAssignment>();

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

