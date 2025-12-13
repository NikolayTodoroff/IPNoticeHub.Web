using IPNoticeHub.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using static IPNoticeHub.Shared.Constants.ValidationConstants.TrademarkSearchFilterConstants;

namespace IPNoticeHub.Web.Models.Trademarks
{
    /// <summary>
    /// Binds and validates the search filter coming from the query string.
    /// </summary>
    public sealed class TrademarkFilterViewModel : IValidatableObject
    {
        [MaxLength(TrademarkSearchTermMaxLength, ErrorMessage = "Search term cannot exceed {0} characters in length.")]
        public string? SearchTerm { get; init; }


        public TrademarkSearchBy SearchBy { get; init; } = TrademarkSearchBy.Wordmark;


        public DataProvider? Provider { get; set; }


        public TrademarkStatusCategory? Status { get; set; }


        public int[] ClassNumbers { get; set; } = Array.Empty<int>();


        public bool ExactMatch { get; set; }


        [Range(TrademarkCurrentPageMinValue, int.MaxValue, ErrorMessage = "Current page number must be {0} or greater.")]
        public int CurrentPage { get; init; } = TrademarkCurrentPageMinValue;


        [Range(TrademarkResultsPerPageMinValue, TrademarkResultsPerPageMaxValue, ErrorMessage = "Results per page must be between {0} and {1}.")]
        public int ResultsPerPage { get; init; } = TrademarkResultsPerPageMinValue;


        /// <summary>
        /// Performs custom validation on the properties of the TrademarkFilterViewModel.
        /// Validates the following:
        /// - Ensures the search term contains only digits and does not exceed the maximum length when searching by "Number".
        /// - Ensures the current page number is not less than the minimum allowed value.
        /// - Ensures the results per page fall within the allowed range.
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            string searchTerm = (SearchTerm ?? string.Empty).Trim();

            if (searchTerm.Length > 0 && SearchBy == TrademarkSearchBy.Number)
            {
                bool onlyDigitsUsed = searchTerm.All(char.IsDigit);

                if (!onlyDigitsUsed || searchTerm.Length > TrademarkSearchTermMaxLength)
                {
                    yield return new ValidationResult($"Application/registration number must contain digits only (max {TrademarkSearchTermMaxLength} characters).",

                        new[] { nameof(SearchTerm) }

                    );
                }
            }

            if (CurrentPage < TrademarkCurrentPageMinValue)
            {
                yield return new ValidationResult($"Current page number must be ≥ {TrademarkCurrentPageMinValue}.",

                    new[] { nameof(CurrentPage) }

                );
            }

            if (ResultsPerPage < TrademarkResultsPerPageMinValue || ResultsPerPage > TrademarkResultsPerPageMaxValue)
            {
                yield return new ValidationResult(

                    $"Results per page must be between {TrademarkResultsPerPageMinValue} and {TrademarkResultsPerPageMaxValue}.",

                    new[] { nameof(ResultsPerPage) }

                );
            }
        }
    }
}


