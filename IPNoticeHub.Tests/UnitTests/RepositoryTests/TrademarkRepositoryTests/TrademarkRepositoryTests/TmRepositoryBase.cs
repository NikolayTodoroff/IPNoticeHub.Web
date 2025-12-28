using IPNoticeHub.Application.Repositories.CopyrightRepository;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Services.TrademarkService.Implementations;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Repositories.TrademarkRepository;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using NuGet.Protocol.Core.Types;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.TrademarkRepositoryTests.TrademarkRepositoryTests
{
    public class TmRepositoryBase
    {
        protected IPNoticeHubDbContext testDbContext = null!;
        protected ITrademarkRepository repository = null!;

        [SetUp]
        public void SetUp()
        {
            testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            repository =
                new TrademarkRepository(testDbContext);
        }
    }
}
