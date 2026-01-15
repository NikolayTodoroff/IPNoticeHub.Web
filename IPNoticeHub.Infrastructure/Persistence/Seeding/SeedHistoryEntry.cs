using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static IPNoticeHub.Shared.Constants.SeedHistoryConstants.SeedHistoryEntryConstants;

namespace IPNoticeHub.Infrastructure.Persistence.Seeding
{
    [Table("SeedHistory")]
    [Index(nameof(SeedKey), nameof(Version), IsUnique = true)]
    public class SeedHistoryEntry
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(SeedKeyMaxLength)]
        public string SeedKey { get; set; } = null!;

        [Required, MaxLength(VersionMaxLength)]
        public string Version { get; set; } = null!;

        [Required]
        public DateTime AppliedOnUtc { get; set; }

        [Required, MaxLength(EnvironmentMaxLength)]
        public string Environment { get; set; } = null!;

        [MaxLength(AppliedByMaxLength)]
        public string? AppliedBy { get; set; }

        [MaxLength(NotesMaxLength)]
        public string? Notes { get; set; }
    }
}
