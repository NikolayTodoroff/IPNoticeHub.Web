using IPNoticeHub.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IPNoticeHub.Infrastructure.Persistence;

namespace IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories
{
    public sealed class TestWebAppFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? activeSQLiteConnection;
        private string? currentUserId;

        public HttpClient CreateClientAs(string userId)
        {
            currentUserId = userId;
            return CreateClient(
                new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    s => s.ServiceType == typeof(DbContextOptions<IPNoticeHubDbContext>));

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                activeSQLiteConnection = 
                new SqliteConnection("DataSource=:memory:");

                activeSQLiteConnection.Open();

                services.AddDbContext<IPNoticeHubDbContext>(optionsBuilder =>
                {
                    optionsBuilder.UseSqlite(activeSQLiteConnection);
                });

                using (var serviceScope = 
                services.BuildServiceProvider().CreateScope())
                {
                    var testDbContext = 
                    serviceScope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();

                    testDbContext.Database.EnsureCreated();
                }

                services.AddSingleton(
                    new TestWebAppFactoryAccessor { Factory = this });

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestAuth";
                    options.DefaultChallengeScheme = "TestAuth";
                })
                .AddScheme<
                    AuthenticationSchemeOptions, 
                    TestAuthHandler>(
                    "TestAuth", 
                    _ => { });

                services.AddSingleton<IAntiforgery, NoOpAntiforgery>();

                services.PostConfigure<MvcViewOptions>(opts =>
                {
                    opts.ViewEngines.Clear();
                    opts.ViewEngines.Add(new NoOpViewEngine());
                });
            });

            builder.UseSetting(
                "Authentication:DefaultScheme", 
                "TestAuth");
        }

        internal string? GetCurrentUserId() => currentUserId;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                activeSQLiteConnection?.Dispose();
                activeSQLiteConnection = null;
            }
        }
    }
}