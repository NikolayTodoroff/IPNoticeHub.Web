using IPNoticeHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Domain.Entities.Copyrights;
using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Domain.Entities.Watchlist;
using IPNoticeHub.Infrastructure.Persistence.Seeding;

namespace IPNoticeHub.Infrastructure.Persistence
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
        public DbSet<Watchlist> Watchlists { get; set; }
        public DbSet<LegalDocument> LegalDocuments { get; set; }
        public DbSet<SeedHistoryEntry> SeedHistoryEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(IPNoticeHubDbContext).Assembly);
        }
    }
}
