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
using Microsoft.Extensions.Hosting;
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
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                // Register SQLite DbContext for testing BEFORE Program.Main runs
                activeSQLiteConnection = new SqliteConnection("DataSource=:memory:");
                activeSQLiteConnection.Open();

                services.AddDbContext<IPNoticeHubDbContext>(options =>
                    options.UseSqlite(activeSQLiteConnection));
            });

            builder.ConfigureTestServices(services =>
            {
                // Add a hosted service to ensure database is created at startup
                services.AddHostedService<DatabaseInitializerService>();

                services.AddSingleton(
                    new TestWebAppFactoryAccessor { Factory = this });

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestAuth";
                    options.DefaultChallengeScheme = "TestAuth";
                }).
                AddScheme<
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

        private class DatabaseInitializerService : IHostedService
        {
            private readonly IServiceProvider _services;

            public DatabaseInitializerService(IServiceProvider services)
            {
                _services = services;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                using var scope = _services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                dbContext.Database.EnsureCreated();
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
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