using IPNoticeHub.Common.EnumConstants;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static IPNoticeHub.Common.ValidationConstants.TrademarkEventConstants;

namespace IPNoticeHub.Data.Entities.TrademarkRegistration
{
    /// <summary>
    /// Represents an event associated with a trademark registration.
    /// This includes details such as the event date, type, code, and description.
    /// </summary>
    public class TrademarkEvent
    {
        [Comment("Primary key for the TrademarkEvent entity")]
        public int Id { get; set; }


        [ForeignKey(nameof(TrademarkRegistration))]
        [Comment("Foreign key referencing the associated TrademarkRegistration")]
        public int TrademarkId { get; set; }


        [Comment("Navigation property for the associated TrademarkRegistration")]
        public TrademarkEntity TrademarkRegistration { get; set; } = null!;


        [Required, Comment("The date when the event occurred")]
        public DateTime EventDate { get; set; }


        [MaxLength(TrademarkEventCodeMaxLength)]
        [Comment("Code representing the event type or category")]
        public string Code { get; set; } = null!;


        [MaxLength(TrademarkEventDescriptionMaxLength)]
        [Comment("Detailed description of the event")]
        public string Description { get; set; } = string.Empty;


        [MaxLength(TrademarkEventTypeRawMaxLength)]
        [Comment("Raw event type code as received from the source system")]
        public string? EventTypeRaw { get; set; }


        [Comment("Enum representing the type of the event")]
        public TrademarkEventType EventType { get; set; } = TrademarkEventType.Other;
    }
}