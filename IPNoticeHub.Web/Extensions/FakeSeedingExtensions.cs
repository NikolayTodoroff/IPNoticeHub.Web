using IPNoticeHub.Infrastructure.Persistence;
using IPNoticeHub.Infrastructure.Persistence.Seeding;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Web.Extensions
{
    public static class FakeSeedingExtensions
    {
        public static async Task SeedFakeDataAsync(this WebApplication app)
        {
            bool seedingEnabled = app.Configuration.GetValue<bool>("Seeding:Enabled");

            if (!seedingEnabled || !app.Environment.IsDevelopment())
            {
                return;
            }

            using var scope = app.Services.CreateScope();

            await SeedFakeDataAsync(
                scopedServices: scope.ServiceProvider,
                environmentName: app.Environment.EnvironmentName,
                seedingEnabled: seedingEnabled,
                seedAction: FakeDataSeeder.SeedAsync
            );
        }

        public static async Task SeedFakeDataAsync(
            IServiceProvider scopedServices,
            string environmentName,
            bool seedingEnabled,
            Func<IServiceProvider, Task>? seedAction = null)
        {
            if (!seedingEnabled || !string.Equals(
                environmentName,
                "Development", 
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            seedAction ??= FakeDataSeeder.SeedAsync;

            var logger = scopedServices.
                GetRequiredService<ILoggerFactory>().
                CreateLogger("FakeSeeding");

            var dbContext = 
                scopedServices.GetRequiredService<IPNoticeHubDbContext>();

            const string seedKey = "FakeData";
            const string seedVersion = "1";

            await using var transaction = 
                await dbContext.Database.BeginTransactionAsync();

            try
            {
                dbContext.Set<SeedHistoryEntry>().Add(new SeedHistoryEntry
                {
                    SeedKey = seedKey,
                    Version = seedVersion,
                    AppliedOnUtc = DateTime.UtcNow,
                    Environment = environmentName,
                    AppliedBy = Environment.MachineName,
                    Notes = "Fake trademarks dataset + demo user " +
                    "trademarks and copyrights collection and trademarks watchlist seeding"
                });

                await dbContext.SaveChangesAsync();

                logger.LogInformation("Fake seeding started: " +
                    "claimed marker ({SeedKey}, v{Version}).", seedKey, seedVersion);

                await seedAction(scopedServices);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                logger.LogInformation("Fake seeding completed: " +
                    "committed ({SeedKey}, v{Version}).", seedKey, seedVersion);
            }
            catch (DbUpdateException ex) when (IsUniqueSeedHistoryViolation(ex))
            {
                logger.LogInformation("Fake seeding skipped: " +
                    "marker already exists ({SeedKey}, v{Version}).", seedKey, seedVersion);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static bool IsUniqueSeedHistoryViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException { Number: 2601 or 2627 };
        }
    }
}
