using IPNoticeHub.Domain.Entities.Trademarks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IPNoticeHub.Infrastructure.Persistence.Configurations
{
    public sealed class TrademarkEntityConfig : IEntityTypeConfiguration<TrademarkEntity>
    {
        public void Configure(EntityTypeBuilder<TrademarkEntity> builder)
        {
            builder.HasIndex(t => new { t.Source, t.SourceId }).
                IsUnique();

            builder.HasIndex(e => new { e.Source, e.SourceId }).
                IsUnique().
                HasDatabaseName("UX_Trademark_Source_SourceId");

            builder.HasIndex(t => t.RegistrationNumber);

            builder.HasIndex(t => t.SourceId);
        }
    }
}
