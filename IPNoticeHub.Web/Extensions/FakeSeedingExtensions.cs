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

            if (!seedingEnabled || !app.Environment.IsDevelopment())  return;

            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            var logger = services.
                GetRequiredService<ILoggerFactory>().
                CreateLogger("FakeSeeding");

            var dbContext = services.GetRequiredService<IPNoticeHubDbContext>();

            const string seedKey = "FakeData";
            const string seedVersion = "1";

            bool alreadySeededDb = 
                await dbContext.Set<SeedHistoryEntry>().
                AsNoTracking().
                AnyAsync(x => x.SeedKey == seedKey && x.Version == seedVersion);

            if (alreadySeededDb)
            {
                logger.LogInformation(
                    "Fake seeding skipped: SeedHistory marker exists ({SeedKey}, v{Version}).",
                    seedKey, seedVersion);

                return;
            }

            await using var tx = await dbContext.Database.BeginTransactionAsync();

            try
            {
                dbContext.Set<SeedHistoryEntry>().Add(new SeedHistoryEntry
                {
                    SeedKey = seedKey,
                    Version = seedVersion,
                    AppliedOnUtc = DateTime.UtcNow,
                    Environment = app.Environment.EnvironmentName,
                    AppliedBy = Environment.MachineName,
                    Notes = "Bogus fake dataset + demo user collection/watchlist seeding"
                });

                await dbContext.SaveChangesAsync();

                logger.LogInformation(
                    "Fake seeding started: applying ({SeedKey}, v{Version}).",
                    seedKey, seedVersion);

                await FakeDataSeeder.SeedAsync(services);

                await dbContext.SaveChangesAsync();

                await tx.CommitAsync();

                logger.LogInformation(
                    "Fake seeding completed: marker saved ({SeedKey}, v{Version}).",
                    seedKey, seedVersion);
            }

            catch (DbUpdateException ex) when (IsUniqueSeedHistoryViolation(ex))
            {
                logger.LogInformation(
                    "Fake seeding skipped: " +
                    "marker was inserted concurrently ({SeedKey}, v{Version}).",
                    seedKey, seedVersion);
            }

            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private static bool IsUniqueSeedHistoryViolation(DbUpdateException ex)
        {
            // SQL Server unique constraint violation numbers:
            // 2601 = Cannot insert duplicate key row in object with unique index
            // 2627 = Violation of UNIQUE KEY constraint
            return ex.InnerException is SqlException { Number: 2601 or 2627 };
        }
    }
}
