using System.Net.Http;
using System.Linq;
using IPNoticeHub.Data;
using IPNoticeHub.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IPNoticeHub.Tests.IntegrationTests.TestUtilities
{
    public sealed class TestWebAppFactory : WebApplicationFactory<Program>
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
                // ---- Replace real DB with SQLite in-memory ----
                var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<IPNoticeHubDbContext>));
                if (descriptor is not null) services.Remove(descriptor);

                services.AddDbContext<IPNoticeHubDbContext>(opts =>
                {
                    opts.UseSqlite($"DataSource={inMemoryTestDb};Mode=Memory;Cache=Shared");
                });

                // Ensure database exists and stays open for the in-memory connection lifetime
                using (var scope = services.BuildServiceProvider().CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<IPNoticeHubDbContext>();
                    db.Database.OpenConnection();
                    db.Database.EnsureCreated();
                }

                // Allow TestAuthHandler to read current user id
                services.AddSingleton(new TestWebAppFactoryAccessor { Factory = this });

                // ---- Fake authentication ----
                services.AddAuthentication("TestAuth")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", _ => { });

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
    }

    
}
