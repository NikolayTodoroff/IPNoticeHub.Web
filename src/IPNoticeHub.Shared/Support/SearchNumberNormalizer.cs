using System.Text;

namespace IPNoticeHub.Shared.Support
{
    public static class SearchNumberNormalizer
    {
        public static string NormalizeSearchNumber(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

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
