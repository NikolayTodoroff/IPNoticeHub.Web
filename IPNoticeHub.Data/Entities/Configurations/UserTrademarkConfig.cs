using IPNoticeHub.Data.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IPNoticeHub.Data.Entities.Configurations
{
    public sealed class UserTrademarkConfig : IEntityTypeConfiguration<UserTrademark>
    {
        public void Configure(EntityTypeBuilder<UserTrademark> builder)
        {
            builder.HasKey(ut => new { ut.UserId, ut.TrademarkId });

            builder.HasOne(ut => ut.User).
                WithMany(u => u.UserTrademarks).
                HasForeignKey(ut => ut.UserId).
                OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ut => ut.Trademark).
                WithMany(t => t.UserTrademarks).
                HasForeignKey(ut => ut.TrademarkId).
                OnDelete(DeleteBehavior.Restrict);
        }
    }
}
