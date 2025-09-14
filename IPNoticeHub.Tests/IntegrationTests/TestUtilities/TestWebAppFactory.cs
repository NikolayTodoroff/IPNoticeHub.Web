using System.Net.Http;
using System.Linq;
using IPNoticeHub.Data;
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

namespace IPNoticeHub.Tests.IntegrationTests.TestUtilities
{
    /// <summary>
    /// Web app factory for integration tests.
    /// - Uses a single shared SQLite in-memory connection kept open for the server lifetime.
    /// - Fakes authentication with a custom handler.
    /// - Replaces MVC view engine with a no-op engine so .cshtml files aren't required.
    /// - Uses a no-op antiforgery so we can post to actions with [ValidateAntiForgeryToken].
    /// </summary>
    public sealed class TestWebAppFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _keepAlive;
        private string? currentUserId;

        /// <summary>
        /// Create an HttpClient authenticated as the given user id.
        /// </summary>
        public HttpClient CreateClientAs(string userId)
        {
            currentUserId = userId;
            return CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // ---- Replace real DbContext with a SINGLE shared SQLite in-memory connection ----
                var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<IPNoticeHubDbContext>));
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                _keepAlive = new SqliteConnection("DataSource=:memory:");
                _keepAlive.Open();

                services.AddDbContext<IPNoticeHubDbContext>(opts =>
                {
                    opts.UseSqlite(_keepAlive);
                });

                // Ensure schema exists while the shared connection is open
                using (var scope = services.BuildServiceProvider().CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                    db.Database.EnsureCreated();
                }

                // Allow TestAuthHandler to read current user id
                services.AddSingleton(new TestWebAppFactoryAccessor { Factory = this });

                // ---- Fake authentication ----
                services.AddAuthentication("TestAuth")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", _ => { });

                // ---- Test-only antiforgery (skip real token validation) ----
                services.AddSingleton<IAntiforgery, NoOpAntiforgery>();

                // ---- Force MVC to use our no-op view engine (no .cshtml needed) ----
                services.PostConfigure<MvcViewOptions>(opts =>
                {
                    opts.ViewEngines.Clear();
                    opts.ViewEngines.Add(new NoOpViewEngine());
                });
            });

            // Use our fake scheme by default
            builder.UseSetting("Authentication:DefaultScheme", "TestAuth");
        }

        internal string? GetCurrentUserId() => currentUserId;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _keepAlive?.Dispose();
                _keepAlive = null;
            }
        }
    }
}