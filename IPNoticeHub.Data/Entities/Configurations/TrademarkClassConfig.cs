using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Data.Entities.TrademarkRegistration;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IPNoticeHub.Data.Entities.Configurations
{
    public sealed class TrademarkClassConfig : IEntityTypeConfiguration<TrademarkClassAssignment>
    {
        public void Configure(EntityTypeBuilder<TrademarkClassAssignment> builder)
        {
            builder.HasKey(tc => 
            new { tc.TrademarkRegistrationId, tc.ClassNumber });

            builder.HasOne(tc => tc.TrademarkRegistration).
                WithMany(t => t.Classes).
                HasForeignKey(tc => tc.TrademarkRegistrationId);
        }
    }
}
