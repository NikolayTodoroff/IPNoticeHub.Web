using IPNoticeHub.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Data
{
    public class IPNoticeHubDbContext : IdentityDbContext<ApplicationUser>
    {
        public IPNoticeHubDbContext(DbContextOptions<IPNoticeHubDbContext> options) : base(options)
        {
        }

        public DbSet<TrademarkRegistration> TrademarkRegistrations { get; set; }
        public DbSet<TrademarkEvent> TrademarkEvents { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TrademarkRegistration>().
                HasIndex(t => new { t.Source, t.SourceId }).
                IsUnique();
        }
    }
}
