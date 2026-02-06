using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IPNoticeHub.Infrastructure.Persistence.Configurations
{
    public sealed class UserTrademarkConfig : IEntityTypeConfiguration<UserTrademark>
    {
        public void Configure(EntityTypeBuilder<UserTrademark> builder)
        {
            builder.
                HasKey(ut => new { 
                ut.ApplicationUserId, 
                ut.TrademarkEntityId 
            });

            builder.
                HasOne<ApplicationUser>().
                WithMany(u => u.UserTrademarks).
                HasForeignKey(ut => ut.ApplicationUserId).
                OnDelete(DeleteBehavior.Restrict);

            builder.
                HasOne(ut => ut.TrademarkEntity).
                WithMany(t => t.UserTrademarks).
                HasForeignKey(ut => ut.TrademarkEntityId).
                OnDelete(DeleteBehavior.Restrict);
        }
    }
}
