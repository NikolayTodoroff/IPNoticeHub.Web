using IPNoticeHub.Domain.Entities.Copyrights;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IPNoticeHub.Infrastructure.Persistence.Configurations
{
    public sealed class CopyrightEntityConfig : IEntityTypeConfiguration<CopyrightEntity>
    {
        public void Configure(EntityTypeBuilder<CopyrightEntity> builder)
        {
            builder.
                HasIndex(c => c.RegistrationNumber).
                IsUnique();
        }
    }
}
