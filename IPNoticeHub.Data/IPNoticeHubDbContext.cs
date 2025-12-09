using IPNoticeHub.Data.Entities.Configurations;
using IPNoticeHub.Data.Entities.CopyrightRegistration;
using IPNoticeHub.Data.Entities.Identity;
using IPNoticeHub.Data.Entities.LegalDocuments;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Data
{
    public class IPNoticeHubDbContext : IdentityDbContext<ApplicationUser>
    {
        public IPNoticeHubDbContext(DbContextOptions<IPNoticeHubDbContext> options) : 
            base(options){}

        public DbSet<TrademarkEntity> TrademarkRegistrations { get; set; }
        public DbSet<TrademarkEvent> TrademarkEvents { get; set; }
        public DbSet<CopyrightEntity> CopyrightRegistrations { get; set; }
        public DbSet<UserTrademark> UserTrademarks { get; set; }
        public DbSet<UserCopyright> UserCopyrights { get; set; }
        public DbSet<UserTrademarkWatchlist> UserTrademarkWatchlists { get; set; }
        public DbSet<LegalDocument> LegalDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(IPNoticeHubDbContext).Assembly);


            // Only seed data if not disabled by an environment variable AND the database is not an in-memory or SQLite test database.
            bool disableSeeding = Environment.GetEnvironmentVariable("IPNOTICEHUB_DISABLE_SEED") == "1";

            if (disableSeeding)
            {
                Console.WriteLine("[DbContext] Seeding disabled via environment variable (IPNOTICEHUB_DISABLE_SEED=1).");
            }
            else if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                Console.WriteLine("[DbContext] Seeding skipped (InMemory provider detected).");
            }
            else if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                Console.WriteLine("[DbContext] Seeding skipped (SQLite provider detected).");
            }
            else
            {
                Console.WriteLine("[DbContext] Running seed data (real relational provider).");
                Seed.FakeDataSeeder.Seed(builder);
            }
        }
    }
}
