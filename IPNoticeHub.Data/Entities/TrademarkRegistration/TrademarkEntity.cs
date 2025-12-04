using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Entities.ApplicationUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Common.ValidationConstants.TrademarkRegistrationConstants;


namespace IPNoticeHub.Data.Entities.TrademarkRegistration
{
    /// <summary>
    /// Represents a trademark registration entity, containing details about the trademark,
    /// its status, associated events, and ownership information.
    /// </summary>
    public class TrademarkEntity
    {
        [Key]
        [Comment("Primary key for the Trademark entity")]
        public int Id { get; set; }


        [Comment("Unique identifier for the Trademark, generated automatically")]
        public Guid PublicId { get; set; } = Guid.NewGuid();


        [Required, MaxLength(WordmarkMaxLength)]
        [Comment("The wordmark or name of the trademark)")]
        public string Wordmark { get; set; } = null!;


        [Required,MaxLength(SourceIdMaxLength)]
        [Comment("Original identifier from the source system (USPTO Serial, EUIPO Application, WIPO IRN)")]
        public string SourceId { get; set; } = null!;


        [MaxLength(RegistrationNumberMaxLength)]
        [Comment("Registration number of the trademark (optional)")]
        public string? RegistrationNumber { get; set; }


        [Required, MaxLength(GoodsAndServicesMaxLength)]
        [Comment("Description of goods and services associated with the trademark")]
        public string GoodsAndServices { get; set; } = null!;


        [Required, MaxLength(OwnerNameMaxLength)]
        [Comment("Name of the current owner/s of the trademark")]
        public string Owner { get; set; } = null!;


        [Required, Comment("Current status of the trademark (default is Pending)")]
        public TrademarkStatusCategory StatusCategory { get; set; } = TrademarkStatusCategory.Pending;


        [Required, MaxLength(TrademarkStatusDetailsMaxLength)]
        public string StatusDetail { get; set; } = null!;


        [Comment("Raw status code from source header (e.g., USPTO <status-code>)")]
        public int? StatusCodeRaw { get; set; }


        [Comment("Raw status date from source header (e.g., USPTO <status-date>) in UTC")]
        public DateTime? StatusDateUtc { get; set; }


        [Comment("Filing date of the trademark application (optional)")]
        public DateTime? FilingDate { get; set; }


        [Comment("Registration date of the trademark (optional)")]
        public DateTime? RegistrationDate { get; set; }


        [MaxLength(MarkImageUrlMaxLength)]
        [Comment("URL of the image representing the trademark (optional)")]
        public string? MarkImageUrl { get; set; }


        [Required, Comment("Source of the data for the trademark registration")]
        public DataProvider Source { get; set; }


        [Comment("Collection of trademark classes associated with this trademark registration")]
        public ICollection<TrademarkClassAssignment> Classes { get; set; } = new List<TrademarkClassAssignment>();


        [Comment("Collection of events related to this trademark registration")]
        public ICollection<TrademarkEvent> Events { get; set; } = new List<TrademarkEvent>();


        [Comment("Navigation property for the many - to - many relationship between TrademarkRegistration and UserTrademark")]
        public ICollection<UserTrademark> UserTrademarks { get; set; } = new List<UserTrademark>();
    }
}
