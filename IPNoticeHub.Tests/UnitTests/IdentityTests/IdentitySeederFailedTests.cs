using FluentAssertions;
using IPNoticeHub.Infrastructure.Identity;
using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Shared.Support;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;

namespace IPNoticeHub.Tests.UnitTests.IdentityTests
{
    public class IdentitySeederFailedTests
    {
        [Test]
        public async Task Seeder_LogsCritical_When_RoleCreation_Fails()
        {
            var mockRoles = MockRoleManager();

            mockRoles.Setup(
                r => r.RoleExistsAsync(
                    It.IsAny<string>())).ReturnsAsync(false);
            
            mockRoles.Setup(r => r.CreateAsync(
                It.IsAny<IdentityRole>())).ReturnsAsync(
                IdentityResult.Failed(new IdentityError { Description = "boom" }));

            var testLogger = new TestLogger<IdentitySeeder>();

            using var serviceProvider = BuildServicesForSeederTest(
                overrides: services =>
                {
                    services.AddScoped(_ => mockRoles.Object);                
                    services.AddScoped<ILogger<IdentitySeeder>>(_ => testLogger);
                });

            await IdentitySeeder.SeedIdentitiesAsync(serviceProvider);

            mockRoles.Verify(r => r.CreateAsync(
                It.Is<IdentityRole>(x => x.Name == RoleNames.Admin)), 
                Times.Once);
            
            mockRoles.Verify(r => r.CreateAsync(
                It.Is<IdentityRole>(x => x.Name == RoleNames.User)), 
                Times.Once);

            testLogger.Entries.Count(
                e => e.level == LogLevel.Critical && 
                e.message.Contains("Failed to create role") && 
                e.message.Contains(RoleNames.Admin)).
                Should().Be(1);

            testLogger.Entries.Count(
                e => e.level == LogLevel.Critical && 
                e.message.Contains("Failed to create role") && 
                e.message.Contains(RoleNames.User)).
                Should().Be(1);
        }

        [Test]
        public async Task Seeder_Skips_Creation_And_LogsNothing_When_Role_Already_Exists()
        {
            var mockRoles = MockRoleManager();

            mockRoles.Setup(
                r => r.RoleExistsAsync(
                    It.IsAny<string>())).ReturnsAsync(true);

            var testLogger = new TestLogger<IdentitySeeder>();

            using var serviceProvider = BuildServicesForSeederTest(
                overrides: services =>
                {
                    services.AddScoped(_ => mockRoles.Object);
                    services.AddScoped<ILogger<IdentitySeeder>>(_ => testLogger);
                });

            await IdentitySeeder.SeedIdentitiesAsync(serviceProvider);

            mockRoles.Verify(r => r.CreateAsync(
                It.IsAny<IdentityRole>()), 
                Times.Never);

            testLogger.Entries.Any(
                e => e.level >= LogLevel.Warning).Should().BeFalse();
        }

        private static ServiceProvider BuildServicesForSeederTest(Action<IServiceCollection> overrides)
        {
            var services = new ServiceCollection();

            services.AddLogging();

            services.AddDbContext<IPNoticeHubDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<IPNoticeHubDbContext>()
                    .AddDefaultTokenProviders();

            overrides(services);

            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>().Database.EnsureCreated();

            return sp;
        }

        private static Mock<RoleManager<IdentityRole>> MockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                store.Object,
                Array.Empty<IRoleValidator<IdentityRole>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null);
        }

        public sealed class TestLogger<T> : ILogger<T>
        {
            public readonly List<(LogLevel level, string message)> Entries = 
                new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?,
                    string> formatter)
                => Entries.Add((logLevel, formatter(state, exception)));

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();
                public void Dispose() { }
            }
        }
    }
}
