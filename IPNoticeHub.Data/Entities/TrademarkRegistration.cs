using Microsoft.EntityFrameworkCore;
using IPNoticeHub.Data.EnumConstants;
using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Common.EntityValidationConstants.TrademarkRegistrationConstants;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPNoticeHub.Data.Entities
{
    public class TrademarkRegistration
    {
        [Key]
        [Comment("Primary key for the Trademark entity")]
        public int Id { get; set; }


        [Comment("Unique identifier for the Trademark, generated automatically")]
        public Guid PublicId { get; set; } = Guid.NewGuid();


        [Required, MaxLength(WordmarkMaxLength)]
        [Comment("The wordmark or name of the trademark)")]
        public string Wordmark { get; set; } = string.Empty;


        [Required,MaxLength(SerialNumberMaxLength)]
        [Comment("The serial number of the trademark application")]
        public string SerialNumber { get; set; } = string.Empty;


        [MaxLength(RegistrationNumberMaxLength)]
        [Comment("Registration number of the trademark (optional)")]
        public string? RegistrationNumber { get; set; }


        [Required, MaxLength(GoodsAndServicesMaxLength)]
        [Comment("Description of goods and services associated with the trademark")]
        public string GoodsAndServices { get; set; } = string.Empty;


        [Required, MaxLength(OwnerNameMaxLength)]
        [Comment("Name of the current owner/s of the trademark")]
        public string Owner { get; set; } = string.Empty;


        [Required, Comment("Current status of the trademark (default is Pending)")]
        public TrademarkStatusCategory StatusCategory { get; set; } = TrademarkStatusCategory.Pending;


        [Required, MaxLength(TrademarkStatusDetailsMaxLength)]
        public string StatusDetail { get; set; } = string.Empty;


        [Comment("Filing date of the trademark application (optional)")]
        public DateTime? FilingDate { get; set; }


        [Comment("Registration date of the trademark (optional)")]
        public DateTime? RegistrationDate { get; set; }


        [Comment("Collection of trademark classes associated with this trademark registration")]
        public ICollection<TrademarkClass> Classes { get; set; } = new List<TrademarkClass>();


        [Comment("Collection of events related to this trademark registration")]
        public ICollection<TrademarkEvent> Events { get; set; } = new List<TrademarkEvent>();


        [Required, ForeignKey(nameof(ApplicationUser))]
        [Comment("Identifier for the user who owns this trademark registration")]
        public string ApplicationUserId { get; set; } = string.Empty;


        [Comment("The application user entity associated with this trademark registration")]
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
