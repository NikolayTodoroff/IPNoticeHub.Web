//using FluentAssertions;
//using IPNoticeHub.Shared.Enums;
//using IPNoticeHub.Data;
//using IPNoticeHub.Domain.Entities.TrademarkRegistration;
//using IPNoticeHub.Tests.IntegrationTests.TestUtilities;
//using Microsoft.Extensions.DependencyInjection;
//using NUnit.Framework;
//using System.Net;

//namespace IPNoticeHub.Tests.IntegrationTests.TrademarkIntegrationTests
//{
//    public class TrademarkIndexIntegrationTests
//    {
//        private TestWebAppFactory appFactory = null!;

//        [SetUp]
//        public void SetUp()
//        {
//            appFactory = new TestWebAppFactory();
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            appFactory.Dispose();
//        }

//        [Test]
//        public async Task Get_Index_NoSearchTerm_Returns200()
//        {
//            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

//            var response = await client.GetAsync("/Trademarks");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//        }

//        [Test]
//        public async Task Get_Index_WithSearchTerm_Returns200()
//        {
//            using (var serviceScope = appFactory.Services.CreateScope())
//            {
//                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

//                testDbContext.TrademarkRegistrations.AddRange(
//                    new TrademarkEntity
//                    {
//                        Wordmark = "AAA1",
//                        SourceId = "US-A-001",
//                        RegistrationNumber = "RN-A1",
//                        GoodsAndServices = "Software; services",
//                        Owner = "AZ",
//                        StatusCategory = TrademarkStatusCategory.Registered,
//                        StatusDetail = "Registered",
//                        Source = DataProvider.USPTO
//                    },
//                    new TrademarkEntity
//                    {
//                        Wordmark = "B2",
//                        SourceId = "US-B-002",
//                        RegistrationNumber = "RN-B2",
//                        GoodsAndServices = "Games; media",
//                        Owner = "BZ",
//                        StatusCategory = TrademarkStatusCategory.Pending,
//                        StatusDetail = "Pending",
//                        Source = DataProvider.USPTO
//                    },
//                    new TrademarkEntity
//                    {
//                        Wordmark = "AAAC3",
//                        SourceId = "US-C-003",
//                        RegistrationNumber = "RN-C1",
//                        GoodsAndServices = "Education; software",
//                        Owner = "CZ",
//                        StatusCategory = TrademarkStatusCategory.Pending,
//                        StatusDetail = "Pending",
//                        Source = DataProvider.USPTO
//                    }
//                );
//                await testDbContext.SaveChangesAsync();
//            }

//            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

//            var response = await client.GetAsync("/Trademarks?SearchTerm=AAA");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//        }

//        [Test]
//        public async Task Get_Index_WithSearch_PagingAndSorting_Returns200()
//        {
//            using (var serviceScope = appFactory.Services.CreateScope())
//            {
//                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

//                testDbContext.TrademarkRegistrations.AddRange(
//                    new TrademarkEntity
//                    { 
//                        Wordmark = "Starstruck",
//                        SourceId = "US-PAGE-A1",
//                        RegistrationNumber = "RN-A",
//                        GoodsAndServices = "Software",
//                        Owner = "Arnold",
//                        StatusCategory = TrademarkStatusCategory.Registered,
//                        StatusDetail = "Registered",
//                        Source = DataProvider.USPTO },

//                    new TrademarkEntity
//                    { 
//                        Wordmark = "Starstruck100",
//                        SourceId = "US-PAGE-B2",
//                        RegistrationNumber = "RN-B",
//                        GoodsAndServices = "Software",
//                        Owner = "Arnold",
//                        StatusCategory = TrademarkStatusCategory.Pending,
//                        StatusDetail = "Pending",
//                        Source = DataProvider.USPTO },

//                    new TrademarkEntity
//                    { 
//                        Wordmark = "StarTrek",
//                        SourceId = "US-PAGE-C3",
//                        RegistrationNumber = "RN-C",
//                        GoodsAndServices = "Software",
//                        Owner = "Arnold",
//                        StatusCategory = TrademarkStatusCategory.Pending,
//                        StatusDetail = "Pending",
//                        Source = DataProvider.USPTO }
//                );
//                await testDbContext.SaveChangesAsync();
//            }

//            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

//            var response = await client.GetAsync("/Trademarks?SearchTerm=Starstruck&sortBy=WordmarkAsc&currentPage=2&resultsPerPage=1");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//        }

//        [Test]
//        public async Task Get_Index_WithNoResults_Returns200()
//        {
//            using (var serviceScope = appFactory.Services.CreateScope())
//            {
//                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

//                testDbContext.TrademarkRegistrations.Add(new TrademarkEntity
//                {
//                    Wordmark = "A1000",
//                    SourceId = "US-NOHIT-1",
//                    RegistrationNumber = "RN-X",
//                    GoodsAndServices = "Games",
//                    Owner = "Beta",
//                    StatusCategory = TrademarkStatusCategory.Registered,
//                    StatusDetail = "Registered",
//                    Source = DataProvider.USPTO
//                });
//                await testDbContext.SaveChangesAsync();
//            }

//            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

//            var response = await client.GetAsync("/Trademarks?SearchTerm=ZZZ");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//        }

//        [Test]
//        public async Task Get_Index_WithOutOfRangePaging_Returns200()
//        {
//            using (var serviceScope = appFactory.Services.CreateScope())
//            {
//                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

//                testDbContext.TrademarkRegistrations.AddRange(
//                    new TrademarkEntity
//                    {
//                        Wordmark = "AAA100",
//                        SourceId = "US-OP-A",
//                        RegistrationNumber = "RN-A",
//                        GoodsAndServices = "Software",
//                        Owner = "AWW LLC",
//                        StatusCategory = TrademarkStatusCategory.Registered,
//                        StatusDetail = "Registered",
//                        Source = DataProvider.USPTO },

//                    new TrademarkEntity
//                    {
//                        Wordmark = "AAA1000",
//                        SourceId = "US-OP-B",
//                        RegistrationNumber = "RN-B",
//                        GoodsAndServices = "Software",
//                        Owner = "AWW LLC",
//                        StatusCategory = TrademarkStatusCategory.Pending,
//                        StatusDetail = "Pending",
//                        Source = DataProvider.USPTO }
//                );
//                await testDbContext.SaveChangesAsync();
//            }

//            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

//            var response = await client.GetAsync("/Trademarks?SearchTerm=AAA100&sortBy=WordmarkAsc&currentPage=9999&resultsPerPage=9999");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//        }

//        [Test]
//        public async Task Get_Index_WithInvalidSortBy_Returns200()
//        {
//            using (var serviceScope = appFactory.Services.CreateScope())
//            {
//                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

//                testDbContext.TrademarkRegistrations.AddRange(
//                    new TrademarkEntity
//                    { 
//                        Wordmark = "D100",
//                        SourceId = "US-SORTBAD-1",
//                        RegistrationNumber = "RN-1",
//                        GoodsAndServices = "Software",
//                        Owner = "Anton B",
//                        StatusCategory = TrademarkStatusCategory.Registered,
//                        StatusDetail = "Registered",
//                        Source = DataProvider.USPTO },

//                    new TrademarkEntity
//                    { 
//                        Wordmark = "D100ZZ",
//                        SourceId = "US-SORTBAD-2",
//                        RegistrationNumber = "RN-2",
//                        GoodsAndServices = "Software",
//                        Owner = "Anton B",
//                        StatusCategory = TrademarkStatusCategory.Pending,
//                        StatusDetail = "Pending",
//                        Source = DataProvider.USPTO }
//                );

//                await testDbContext.SaveChangesAsync();
//            }

//            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

//            var response = await client.GetAsync("/Trademarks?SearchTerm=D100&sortBy=TotallyNotAValue&currentPage=1&resultsPerPage=10");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//        }

//        [TestCase(0)]
//        [TestCase(-5)]
//        public async Task Get_Index_WhenResultsPerPage_NonPositive_Returns200(int resultsPerPage)
//        {
//            using (var serviceScope = appFactory.Services.CreateScope())
//            {
//                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

//                testDbContext.TrademarkRegistrations.AddRange(
//                    new TrademarkEntity
//                    { 
//                        Wordmark = "AA",
//                        SourceId = "US-RPPZ-1",
//                        RegistrationNumber = "RN-1",
//                        GoodsAndServices = "Software",
//                        Owner = "Acme",
//                        StatusCategory = TrademarkStatusCategory.Registered,
//                        StatusDetail = "Registered",
//                        Source = DataProvider.USPTO },

//                    new TrademarkEntity
//                    { 
//                        Wordmark = "AAZ",
//                        SourceId = "US-RPPZ-2",
//                        RegistrationNumber = "RN-2",
//                        GoodsAndServices = "Software",
//                        Owner = "Acme",
//                        StatusCategory = TrademarkStatusCategory.Pending,
//                        StatusDetail = "Pending",
//                        Source = DataProvider.USPTO }
//                );
//                await testDbContext.SaveChangesAsync();
//            }

//            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

//            var url = $"/Trademarks?SearchTerm=ALPHA&sortBy=WordmarkAsc&currentPage=1&resultsPerPage={resultsPerPage}";
//            var response = await client.GetAsync(url);

//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//        }

//        [TestCase(5000)]
//        [TestCase(10000)]
//        public async Task Get_Index_WhenResultsPerPage_VeryLarge_Returns200(int resultsPerPage)
//        {
//            using (var serviceScope = appFactory.Services.CreateScope())
//            {
//                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

//                testDbContext.TrademarkRegistrations.AddRange(
//                    new TrademarkEntity
//                    {
//                        Wordmark = "ALPHA",
//                        SourceId = "US-RPPL-1",
//                        RegistrationNumber = "RN-1",
//                        GoodsAndServices = "Software",
//                        Owner = "Acme",
//                        StatusCategory = TrademarkStatusCategory.Registered,
//                        StatusDetail = "Registered",
//                        Source = DataProvider.USPTO },

//                    new TrademarkEntity
//                    {
//                        Wordmark = "ALPHABET",
//                        SourceId = "US-RPPL-2",
//                        RegistrationNumber = "RN-2",
//                        GoodsAndServices = "Software",
//                        Owner = "Acme",
//                        StatusCategory = TrademarkStatusCategory.Pending,
//                        StatusDetail = "Pending",
//                        Source = DataProvider.USPTO },

//                    new TrademarkEntity
//                    {   Wordmark = "ALPHONSO",
//                        SourceId = "US-RPPL-3",
//                        RegistrationNumber = "RN-3",
//                        GoodsAndServices = "Software",
//                        Owner = "Acme",
//                        StatusCategory = TrademarkStatusCategory.Pending,
//                        StatusDetail = "Pending",
//                        Source = DataProvider.USPTO }
//                );

//                await testDbContext.SaveChangesAsync();
//            }

//            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

//            var url = $"/Trademarks?SearchTerm=ALPHA&sortBy=WordmarkAsc&currentPage=1&resultsPerPage={resultsPerPage}";
//            var response = await client.GetAsync(url);

//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//        }

//        [TestCase("RegistrationDateAsc")]
//        [TestCase("RegistrationDateDesc")]
//        public async Task Get_Index_SortBy_RegistrationDateVariants_Returns200(string sortBy)
//        {
//            using (var serviceScope = appFactory.Services.CreateScope())
//            {
//                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

//                var initianEntity = new TrademarkEntity
//                {
//                    Wordmark = "Initial WM",
//                    SourceId = "US-IX-RD-1",
//                    RegistrationNumber = "RN-OLD",
//                    GoodsAndServices = "Software",
//                    Owner = "AZ LLC",
//                    StatusCategory = TrademarkStatusCategory.Registered,
//                    StatusDetail = "Registered",
//                    RegistrationDate = new DateTime(2020, 1, 1),
//                    Source = DataProvider.USPTO
//                };

//                var newEntity = new TrademarkEntity
//                {
//                    Wordmark = "New WM",
//                    SourceId = "US-IX-RD-2",
//                    RegistrationNumber = "RN-NEW",
//                    GoodsAndServices = "Software",
//                    Owner = "AZ LLC",
//                    StatusCategory = TrademarkStatusCategory.Registered,
//                    StatusDetail = "Registered",
//                    RegistrationDate = new DateTime(2022, 6, 15),
//                    Source = DataProvider.USPTO
//                };

//                var noRegDateEntity = new TrademarkEntity
//                {
//                    Wordmark = "No RegDate WM",
//                    SourceId = "US-IX-RD-3",
//                    RegistrationNumber = "RN-ND",
//                    GoodsAndServices = "Software",
//                    Owner = "AZ LLC",
//                    StatusCategory = TrademarkStatusCategory.Pending,
//                    StatusDetail = "Pending",
//                    RegistrationDate = null,
//                    Source = DataProvider.USPTO
//                };

//                testDbContext.TrademarkRegistrations.AddRange(initianEntity, newEntity, noRegDateEntity);
//                await testDbContext.SaveChangesAsync();
//            }

//            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

//            var url = $"/Trademarks?SearchTerm=E&sortBy={sortBy}&currentPage=1&resultsPerPage=10";
//            var response = await client.GetAsync(url);

//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//        }

//        [Test]
//        public async Task Get_Index_WhitespaceSearchTerm_Returns200()
//        {
//            using (var serviceScope = appFactory.Services.CreateScope())
//            {
//                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
//                testDbContext.TrademarkRegistrations.AddRange(
//                    new TrademarkEntity
//                    { 
//                        Wordmark = "ALPHA",
//                        SourceId = "US-WS-1",
//                        RegistrationNumber = "RN-1",
//                        GoodsAndServices = "Software",
//                        Owner = "Acme",
//                        StatusCategory = TrademarkStatusCategory.Registered, 
//                        StatusDetail = "Registered",
//                        Source = DataProvider.USPTO },

//                    new TrademarkEntity 
//                    { 
//                        Wordmark = "BRAVO",
//                        SourceId = "US-WS-2", 
//                        RegistrationNumber = "RN-2",
//                        GoodsAndServices = "Games",
//                        Owner = "Beta", 
//                        StatusCategory = TrademarkStatusCategory.Pending,
//                        StatusDetail = "Pending", 
//                        Source = DataProvider.USPTO }
//                );
//                await testDbContext.SaveChangesAsync();
//            }

//            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

//            var response = await client.GetAsync("/Trademarks?SearchTerm=%20%20%20");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//        }

//        [Test]
//        public async Task Get_Index_LargeDataset_Paging_Returns200()
//        {
//            // Arrange
//            using (var serviceScope = appFactory.Services.CreateScope())
//            {
//                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

//                var bulkEntities = new List<TrademarkEntity>(200);
//                for (int i = 0; i < 200; i++)
//                {
//                    bulkEntities.Add(new TrademarkEntity
//                    {
//                        Wordmark = $"BULK-{i:D3}",
//                        SourceId = $"US-IX-BULK-{i:D5}",
//                        RegistrationNumber = $"RN-{i:D6}",
//                        GoodsAndServices = "Software; services",
//                        Owner = "Bulk Inc.",
//                        StatusCategory = TrademarkStatusCategory.Registered,
//                        StatusDetail = "Registered",
//                        Source = DataProvider.USPTO
//                    });
//                }

//                testDbContext.TrademarkRegistrations.AddRange(bulkEntities);
//                await testDbContext.SaveChangesAsync();
//            }

//            var client = appFactory.CreateClient(new() { AllowAutoRedirect = false });

//            var response = await client.GetAsync("/Trademarks?SearchTerm=BULK&sortBy=WordmarkAsc&currentPage=3&resultsPerPage=25");

//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//        }
//    }
//}
