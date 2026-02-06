using IPNoticeHub.Domain.Entities.Identity;
using IPNoticeHub.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IPNoticeHub.Infrastructure.Persistence.Configurations
{
    public sealed class UserCopyrightConfig : IEntityTypeConfiguration<UserCopyright>
    {
        public void Configure(EntityTypeBuilder<UserCopyright> builder)
        {
            builder.
                HasKey(
                uc => new { 
                    uc.ApplicationUserId, 
                    uc.CopyrightEntityId 
                });

            builder.
                HasOne<ApplicationUser>().
                WithMany(u => u.UserCopyrights).
                HasForeignKey(ut => ut.ApplicationUserId).
                OnDelete(DeleteBehavior.Restrict);

            builder.
                HasOne(uc => uc.CopyrightEntity).
                WithMany(ce => ce.UserCopyrights).
                HasForeignKey(uc => uc.CopyrightEntityId).
                OnDelete(DeleteBehavior.Restrict);

            builder.
                HasIndex(
                x => new { x.ApplicationUserId, x.CopyrightEntityId }).
                IsUnique();
        }
    }
}
