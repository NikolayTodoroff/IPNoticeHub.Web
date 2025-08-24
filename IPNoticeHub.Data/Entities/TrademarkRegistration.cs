using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Data.EnumConstants;
using System.ComponentModel.DataAnnotations;

namespace IPNoticeHub.Data.Entities
{
    public class Trademark
    {
        [Key]
        [Comment("Primary key for the Trademark entity")]
        public int Id { get; set; }


        [Comment("Unique identifier for the Trademark, generated automatically")]
        public Guid PublicId { get; set; } = Guid.NewGuid();


        [Required]
        [MaxLength(200)]
        [Comment("The wordmark or name of the trademark (required, max length 200)")]
        public string Wordmark { get; set; } = string.Empty;


        [Required]
        [MaxLength(50)]
        [Comment("The serial number of the trademark application (required, max length 50)")]
        public string SerialNumber { get; set; } = string.Empty;


        [MaxLength(50)]
        [Comment("Registration number of the trademark (optional, max length 50)")]
        public string? RegistrationNumber { get; set; }


        [Comment("Current status of the trademark (default is Pending)")]
        public TrademarkStatus Status { get; set; } = TrademarkStatus.Pending;


        [Comment("Status code from the TSDR system (optional)")]
        public int? TsdrStatusCode { get; set; }


        [StringLength(500)]
        [Comment("Status text from the TSDR system (optional, max length 500)")]
        public string? TsdrStatusText { get; set; }


        [Comment("Classification of the trademark")]
        public TrademarkClass Class { get; set; }


        [Comment("Filing date of the trademark application (optional)")]
        public DateTime? FilingDate { get; set; }


        [Comment("Registration date of the trademark (optional)")]
        public DateTime? RegistrationDate { get; set; }


        [Comment("Collection of associated trademark registration classes")]
        public ICollection<TrademarkRegistrationClass> Classes { get; set; } = new List<TrademarkRegistrationClass>();


        [Comment("Collection of events related to the trademark")]
        public ICollection<TrademarkEvent> Events { get; set; } = new List<TrademarkEvent>();


        [Comment("Indicates whether the trademark is marked as deleted")]
        public bool IsDeleted { get; set; }
    }
}
