using IPNoticeHub.Infrastructure.Persistence.Seeding;

namespace IPNoticeHub.Web.Extensions
{
    public static class FakeSeedingExtensions
    {
        public static async Task SeedFakeDataAsync(this WebApplication app)
        {
            bool seedingEnabled = app.Configuration.GetValue<bool>("Seeding:Enabled");

            if (!seedingEnabled || !app.Environment.IsDevelopment())  return;

            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            var logger = services.
                GetRequiredService<ILoggerFactory>().
                CreateLogger("FakeSeeding");

            logger.LogInformation("Fake seeding is enabled. Running FakeDataSeeder.");
            await FakeDataSeeder.SeedAsync(services);
        }
    }
}
