using IPNoticeHub.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPNoticeHub.Infrastructure.Persistence.Configurations
{
    public class SeedHistoryEntryConfig : IEntityTypeConfiguration<SeedHistoryEntry>
    {
        public void Configure(EntityTypeBuilder<SeedHistoryEntry> builder)
        {
            throw new NotImplementedException();
        }
    }
}
