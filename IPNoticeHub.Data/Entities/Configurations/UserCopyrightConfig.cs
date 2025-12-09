using IPNoticeHub.Data.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IPNoticeHub.Data.Entities.Configurations
{
    public sealed class UserCopyrightConfig : IEntityTypeConfiguration<UserCopyright>
    {
        public void Configure(EntityTypeBuilder<UserCopyright> builder)
        {
            builder.HasKey(uc => 
            new { uc.ApplicationUserId, uc.CopyrightRegistrationId });

            builder.HasOne(uc => uc.ApplicationUser).
                WithMany(u => u.UserCopyrights).
                HasForeignKey(uc => uc.ApplicationUserId).
                OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(uc => uc.CopyrightRegistration).
                WithMany(c => c.UserCopyrights).
                HasForeignKey(uc => uc.CopyrightRegistrationId).
                OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => 
            new { x.ApplicationUserId, x.CopyrightRegistrationId }).
                IsUnique();
        }
    }
}
