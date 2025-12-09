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

            //Seed.FakeDataSeeder.Seed(builder);
        }
    }
}
