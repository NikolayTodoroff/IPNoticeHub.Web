using IPNoticeHub.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static IPNoticeHub.Shared.Constants.ValidationConstants.TrademarkEventConstants;

namespace IPNoticeHub.Domain.Entities.Trademarks
{
    public class TrademarkEvent
    {
        public int Id { get; set; }

        [ForeignKey(nameof(TrademarkRegistration))]
        public int TrademarkId { get; set; }

        public TrademarkEntity TrademarkRegistration { get; set; } = null!;

        [Required]
        public DateTime EventDate { get; set; }

        [MaxLength(TrademarkEventCodeMaxLength)]
        public string Code { get; set; } = null!;

        [MaxLength(TrademarkEventDescriptionMaxLength)]
        public string Description { get; set; } = null!;

        [MaxLength(TrademarkEventTypeRawMaxLength)]
        public string? EventTypeRaw { get; set; }

        public TrademarkEventType EventType { get; set; } = TrademarkEventType.Other;
    }
}