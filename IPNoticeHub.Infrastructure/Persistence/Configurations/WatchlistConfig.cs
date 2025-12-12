using IPNoticeHub.Domain.Entities.Watchlist;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IPNoticeHub.Infrastructure.Persistence.Configurations
{
    public sealed class WatchlistConfig : IEntityTypeConfiguration<Watchlist>
    {
        public void Configure(EntityTypeBuilder<Watchlist> builder)
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
