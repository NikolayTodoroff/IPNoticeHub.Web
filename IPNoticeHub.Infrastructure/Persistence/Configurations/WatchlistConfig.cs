using IPNoticeHub.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IPNoticeHub.Infrastructure.Persistence.Configurations
{
    public sealed class WatchlistConfig : IEntityTypeConfiguration<UserTrademarkWatchlist>
    {
        public void Configure(EntityTypeBuilder<UserTrademarkWatchlist> builder)
        {
            builder.Property(x => x.NotificationsEnabled).
                HasDefaultValue(false);

            builder.HasIndex(x => new { 
                x.UserId, 
                x.TrademarkId }).
                HasFilter("[IsDeleted] = 0").
                IsUnique();

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
