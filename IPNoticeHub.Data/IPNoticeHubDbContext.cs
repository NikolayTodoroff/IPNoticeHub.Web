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
        public IPNoticeHubDbContext(DbContextOptions<IPNoticeHubDbContext> options) : base(options)
        {
        }
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

            builder.Entity<TrademarkEntity>().
                HasIndex(t => new { t.Source, t.SourceId }).
                IsUnique();

            builder.Entity<TrademarkEntity>().
                HasIndex(e => new { e.Source, e.SourceId }).
                IsUnique().
                HasDatabaseName("UX_Trademark_Source_SourceId");

            builder.Entity<CopyrightEntity>().
                HasIndex(c => c.RegistrationNumber).
                IsUnique();

            builder.Entity<UserTrademark>().
                HasKey(ut => new { ut.UserId, ut.TrademarkId });

            builder.Entity<TrademarkEntity>().
                HasIndex(t => t.RegistrationNumber);

            builder.Entity<TrademarkEntity>().
                HasIndex(t => t.SourceId);

            builder.Entity<UserTrademark>().
                HasOne(ut => ut.User).
                WithMany(u => u.UserTrademarks).
                HasForeignKey(ut => ut.UserId).
                OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserTrademark>().
                HasOne(ut => ut.Trademark).
                WithMany(t => t.UserTrademarks).
                HasForeignKey(ut => ut.TrademarkId).
                OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserCopyright>().
                HasKey(uc => new { uc.ApplicationUserId, uc.CopyrightRegistrationId });

            builder.Entity<UserCopyright>().
                HasOne(uc => uc.ApplicationUser).
                WithMany(u => u.UserCopyrights).
                HasForeignKey(uc => uc.ApplicationUserId).
                OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserCopyright>().
                HasOne(uc => uc.CopyrightRegistration).
                WithMany(c => c.UserCopyrights).
                HasForeignKey(uc => uc.CopyrightRegistrationId).
                OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserCopyright>().
                HasIndex(x => new { x.ApplicationUserId, x.CopyrightRegistrationId }).
                IsUnique();

            builder.Entity<UserTrademarkWatchlist>().
                Property(x => x.NotificationsEnabled).
                HasDefaultValue(false);

            builder.Entity<UserTrademarkWatchlist>().
                HasIndex(x => new { x.UserId, x.TrademarkId }).
                HasFilter("[IsDeleted] = 0").
                IsUnique();

            builder.Entity<UserTrademarkWatchlist>().
                HasQueryFilter(x => !x.IsDeleted);

            builder.Entity<TrademarkClassAssignment>().
                HasKey(tc => new { tc.TrademarkRegistrationId, tc.ClassNumber });

            builder.Entity<TrademarkClassAssignment>().
                HasOne(tc => tc.TrademarkRegistration).
                WithMany(t => t.Classes).
                HasForeignKey(tc => tc.TrademarkRegistrationId);

            builder.Entity<LegalDocument>().
                HasOne(d => d.User).
                WithMany(u => u.Documents).
                OnDelete(DeleteBehavior.NoAction);

            builder.Entity<LegalDocument>()
            .HasQueryFilter(d => !d.IsDeleted);


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
