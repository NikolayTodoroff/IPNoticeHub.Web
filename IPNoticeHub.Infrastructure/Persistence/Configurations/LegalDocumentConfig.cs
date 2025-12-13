using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IPNoticeHub.Infrastructure.Persistence.Configurations
{
    public sealed class LegalDocumentConfig : IEntityTypeConfiguration<LegalDocument>
    {
        public void Configure(EntityTypeBuilder<LegalDocument> builder)
        {
            builder.HasKey(ld => ld.LegalDocumentId);

            builder.HasOne<ApplicationUser>()
                .WithMany(u => u.LegalDocuments)
                .HasForeignKey(u => u.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(d => !d.IsDeleted);
        }
    }
}
