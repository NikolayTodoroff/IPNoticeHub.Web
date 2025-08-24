using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Data.EnumConstants;
using System.ComponentModel.DataAnnotations;

namespace IPNoticeHub.Data.Entities
{
    public class TrademarkEvent
    {
        [Comment("Primary key for the TrademarkEvent table")]
        public int Id { get; set; }

        [Comment("Foreign key referencing the TrademarkRegistration table")]
        public int TrademarkRegistrationId { get; set; }

        [Comment("Navigation property for the associated TrademarkRegistration")]
        public TrademarkRegistration Registration { get; set; } = null!;

        [Comment("Type of the trademark event")]
        public TrademarkEventType Type { get; set; }

        [Comment("Date and time when the event occurred")]
        public DateTime OccurredOn { get; set; }

        [StringLength(500)]
        [Comment("Source of the event, e.g., 'TSDR', 'Bulk XML 2025-08-10'")]
        public string? Source { get; set; }

        [StringLength(2000)]
        [Comment("Original text or line from the feed")]
        public string? RawText { get; set; }

        [StringLength(1000)]
        [Comment("URL linking to the original notice or entry, if available")]
        public string? DocumentUrl { get; set; }
    }
}