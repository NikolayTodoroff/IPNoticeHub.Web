using IPNoticeHub.Data.Entities.LegalDocuments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IPNoticeHub.Data.Entities.Configurations
{
    public sealed class LegalDocumentConfig : IEntityTypeConfiguration<LegalDocument>
    {
        public void Configure(EntityTypeBuilder<LegalDocument> builder)
        {
            builder.HasOne(d => d.User).
                WithMany(u => u.Documents).
                OnDelete(DeleteBehavior.NoAction);

            builder.HasQueryFilter(d => !d.IsDeleted);
        }
    }
}
