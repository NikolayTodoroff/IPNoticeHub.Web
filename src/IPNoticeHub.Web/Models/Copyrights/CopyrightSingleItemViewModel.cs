namespace IPNoticeHub.Web.Models.Copyrights
{
    public class CopyrightSingleItemViewModel
    {
        public int Id { get; init; }


        public Guid PublicId { get; init; }


        public string Owner { get; init; } = string.Empty;


        public string RegistrationNumber { get; init; } = string.Empty;


        public string TypeOfWork { get; init; } = string.Empty;


        public string Title { get; init; } = string.Empty;


        public int? YearOfCreation { get; init; }


        public DateTime? DateOfPublication { get; init; }


        public DateTime DateAdded { get; init; }
    }
}
