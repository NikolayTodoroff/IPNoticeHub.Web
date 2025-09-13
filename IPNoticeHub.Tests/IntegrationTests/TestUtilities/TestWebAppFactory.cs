using IPNoticeHub.Data;
using IPNoticeHub.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IPNoticeHub.Tests.IntegrationTests.TestUtilities
{
    public class TestWebAppFactory : WebApplicationFactory<Program>
    {
        private readonly string inMemoryTestDb = $"testdb_{Guid.NewGuid()}";
        private string? currentUserId;

        public HttpClient CreateClientAs(string userId)
        {
            currentUserId = userId;
            return CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // 1) Replace the real DB with SQLite in-memory (closer to EF Core reality than InMemory provider)
                var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<IPNoticeHubDbContext>));
                if (descriptor is not null) services.Remove(descriptor);

                services.AddDbContext<IPNoticeHubDbContext>(opts =>
                {
                    opts.UseSqlite($"DataSource={inMemoryTestDb};Mode=Memory;Cache=Shared");
                });

                // Ensure database is created for each test run
                using var serviceScope = services.BuildServiceProvider().CreateScope();

                var testDbContext = serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                testDbContext.Database.OpenConnection();
                testDbContext.Database.EnsureCreated();
                services.AddSingleton(new TestWebAppFactoryAccessor { Factory = this });

                // 2) Fake authentication so [Authorize] passes and TryGetUserId works
                services.AddAuthentication("TestAuth")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", _ => { });
            });

            // 3) Use our fake scheme by default
            builder.UseSetting("Authentication:DefaultScheme", "TestAuth");
        }

        // Small hook so the handler can read the current user id
        internal string? GetCurrentUserId() => currentUserId;
    }
}

