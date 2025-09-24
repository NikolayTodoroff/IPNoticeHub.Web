using System.Text;

namespace IPNoticeHub.Common.Infrastructure
{
    public static class SearchNumberNormalizer
    {
        /// <summary>
        /// Normalize a registration or serial/application number so comparisons are consistent.
        /// </summary>
        public static string NormalizeSearchNumber(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            StringBuilder? stringOutput = new StringBuilder(input.Length);

            foreach (char character in input)
            {
                if (char.IsLetterOrDigit(character))
                {
                    stringOutput.Append(char.ToUpperInvariant(character));
                }     
            }
            return stringOutput.ToString();
        }
    }
}
