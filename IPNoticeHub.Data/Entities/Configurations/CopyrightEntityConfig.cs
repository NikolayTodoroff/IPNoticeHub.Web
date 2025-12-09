using IPNoticeHub.Data.Entities.CopyrightRegistration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IPNoticeHub.Data.Entities.Configurations
{
    public sealed class CopyrightEntityConfig : IEntityTypeConfiguration<CopyrightEntity>
    {
        public void Configure(EntityTypeBuilder<CopyrightEntity> builder)
        {
            builder.HasIndex(c => c.RegistrationNumber).
               IsUnique();
        }
    }
}
