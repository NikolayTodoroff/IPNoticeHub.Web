using IPNoticeHub.Data.Entities.ApplicationUser;
using IPNoticeHub.Data.Entities.CopyrightRegistration;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ensure that the combination of Source and SourceId is unique for TrademarkRegistration
            builder.Entity<TrademarkEntity>().
                HasIndex(t => new { t.Source, t.SourceId }).
                IsUnique();

            // Ensure that the RegistrationNumber is unique for CopyrightRegistration
            builder.Entity<CopyrightEntity>().
                HasIndex(c => c.RegistrationNumber).
                IsUnique();

            // Define composite primary key for UserTrademark entity
            builder.Entity<UserTrademark>().
                HasKey(ut => new { ut.ApplicationUserId, ut.TrademarkRegistrationId });

            // Adding indexes to improve query performance for RegistrationNumber and SourceId in TrademarkEntity
            builder.Entity<TrademarkEntity>().HasIndex(t => t.RegistrationNumber);
            builder.Entity<TrademarkEntity>().HasIndex(t => t.SourceId);

            // Configure relationship between UserTrademark and ApplicationUser
            builder.Entity<UserTrademark>().
                HasOne(ut => ut.ApplicationUser).
                WithMany(u => u.UserTrademarks).
                HasForeignKey(ut => ut.ApplicationUserId).
                OnDelete(DeleteBehavior.Restrict);

            // Configure relationship between UserTrademark and TrademarkRegistration
            builder.Entity<UserTrademark>().
                HasOne(ut => ut.TrademarkRegistration).
                WithMany(t => t.UserTrademarks).
                HasForeignKey(ut => ut.TrademarkRegistrationId).
                OnDelete(DeleteBehavior.Restrict);

            // Define composite primary key for UserCopyright entity
            builder.Entity<UserCopyright>().
                HasKey(uc => new { uc.ApplicationUserId, uc.CopyrightRegistrationId });

            // Configure relationship between UserCopyright and ApplicationUser
            builder.Entity<UserCopyright>().
                HasOne(uc => uc.ApplicationUser).
                WithMany(u => u.UserCopyrights).
                HasForeignKey(uc => uc.ApplicationUserId).
                OnDelete(DeleteBehavior.Restrict);

            // Configure relationship between UserCopyright and CopyrightRegistration
            builder.Entity<UserCopyright>().
                HasOne(uc => uc.CopyrightRegistration).
                WithMany(c => c.UserCopyrights).
                HasForeignKey(uc => uc.CopyrightRegistrationId).
                OnDelete(DeleteBehavior.Restrict);

            // Define composite primary key for TrademarkClassAssignment entity
            builder.Entity<TrademarkClassAssignment>().
                HasKey(tc => new { tc.TrademarkRegistrationId, tc.ClassNumber });

            // Configure relationship between TrademarkClassAssignment and TrademarkRegistration
            builder.Entity<TrademarkClassAssignment>().
                HasOne(tc => tc.TrademarkRegistration).
                WithMany(t => t.Classes).
                HasForeignKey(tc => tc.TrademarkRegistrationId);

            // Ensure that the combination of ApplicationUserId and CopyrightRegistrationId is unique for UserCopyright
            builder.Entity<UserCopyright>().
                HasIndex(x => new { x.ApplicationUserId, x.CopyrightRegistrationId }).
                IsUnique();

            //Only seed in Debug builds (not in production)
            Seed.FakeDataSeeder.Seed(builder);
        }
    }
}
