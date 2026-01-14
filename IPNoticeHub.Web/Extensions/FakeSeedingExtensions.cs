using IPNoticeHub.Infrastructure.Persistence.Seeding;

namespace IPNoticeHub.Web.Extensions
{
    public static class FakeSeedingExtensions
    {
        public static async Task SeedFakeDataAsync(this WebApplication app)
        {
            bool enabled = app.Configuration.GetValue<bool>("Seeding:Enabled");

            if (!enabled || !app.Environment.IsDevelopment()) return;

            var logger = app.Services.
                GetRequiredService<ILoggerFactory>().
                CreateLogger("FakeSeeding");

            logger.LogInformation("Fake seeding is enabled. Running FakeDataSeeder.");
            await FakeDataSeeder.SeedAsync(app.Services);
        }
    }
}
