using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Domain.Entities.Copyrights;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Identity;

namespace IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories
{
    public static class InMemoryDbContextFactory
    {
        public static IPNoticeHubDbContext CreateTestDbContext(string? dbContextName = null)
        {
            DbContextOptions<IPNoticeHubDbContext>? options = 
                new DbContextOptionsBuilder<IPNoticeHubDbContext>().
                UseInMemoryDatabase(dbContextName ?? Guid.NewGuid().ToString()).
                EnableSensitiveDataLogging().
                Options;

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

        public static TrademarkEntity CreateTrademarkEntity(
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
            var (entity, _) = CreateTrademark(
            wordmark,
            owner,
            goodsAndServices,
            sourceId,
            statusDetail,
            regNumber,
            status,
            source,
            filingDate, classNumbers);

            return entity;
        }

        public static CopyrightEntity CreateCopyright(
            string registrationNumber,
            string title,
            string typeOfWork,
            string owner,
            int? yearOfCreation,
            DateTime? dateOfPublication,
            string? nationOfFirstPublication)
        {
            return new CopyrightEntity
            {
                RegistrationNumber = registrationNumber,
                TypeOfWork = typeOfWork,
                Title = title,
                YearOfCreation = yearOfCreation,
                DateOfPublication = dateOfPublication,
                Owner = owner,
                NationOfFirstPublication = nationOfFirstPublication
            };
        }
        public static ApplicationUser CreateApplicationUser(string id = "1234567")
        {
            return new ApplicationUser
            {
                Id = id,
                UserName = "user1",
                Email = "user@test.com"
            };
        }
    }
}

