namespace IPNoticeHub.Web.Models.Application
{
    public class TreademarkSearchResultSingleItemViewModel
    {
        public int Id { get; init; }


        public Guid PublicId { get; init; }


        public string? RegistrationNumber { get; init; }


        public string Wordmark { get; init; } = string.Empty;


        public string Owner { get; init; } = string.Empty;


        public string Status { get; init; } = string.Empty;


        public DateTime? RegistrationDate { get; init; }
    }
}
