using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Data.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Shared.Constants.ValidationConstants.TrademarkRegistrationConstants;


namespace IPNoticeHub.Data.Entities.TrademarkRegistration
{
    public class TrademarkEntity
    {
        [Key]
        public int Id { get; set; }

        public Guid PublicId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(WordmarkMaxLength)]
        public string Wordmark { get; set; } = null!;

        [Required,MaxLength(SourceIdMaxLength)]
        public string SourceId { get; set; } = null!;

        [MaxLength(RegistrationNumberMaxLength)]
        public string? RegistrationNumber { get; set; }

        [Required, MaxLength(GoodsAndServicesMaxLength)]
        public string GoodsAndServices { get; set; } = null!;

        [Required, MaxLength(OwnerNameMaxLength)]
        public string Owner { get; set; } = null!;

        public TrademarkStatusCategory StatusCategory { get; set; } = 
            TrademarkStatusCategory.Pending;

        [Required, MaxLength(TrademarkStatusDetailsMaxLength)]
        public string StatusDetail { get; set; } = null!;

        public int? StatusCodeRaw { get; set; }

        public DateTime? StatusDateUtc { get; set; }

        public DateTime? FilingDate { get; set; }

        public DateTime? RegistrationDate { get; set; }

        [MaxLength(MarkImageUrlMaxLength)]
        public string? MarkImageUrl { get; set; }

        [Required]
        public DataProvider Source { get; set; }

        public ICollection<TrademarkClassAssignment> Classes { get; set; } = 
            new List<TrademarkClassAssignment>();

        public ICollection<TrademarkEvent> Events { get; set; } = 
            new List<TrademarkEvent>();

        public ICollection<UserTrademark> UserTrademarks { get; set; } = 
            new List<UserTrademark>();
    }
}
