using IPNoticeHub.Common.EnumConstants;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace IPNoticeHub.Data.Entities
{
    public class TrademarkEvent
    {
        public int Id { get; set; }

        public int TrademarkId { get; set; }
        public TrademarkRegistration TrademarkRegistration { get; set; } = null!;

        [Required]
        public DateTime EventDate { get; set; }

        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(2)]
        public string? EventTypeRaw { get; set; }

        public TrademarkEventType EventType { get; set; } = TrademarkEventType.Other;
    }
}