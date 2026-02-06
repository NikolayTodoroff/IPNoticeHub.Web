using IPNoticeHub.Application.Rendering.Abstractions;
using System.Text.RegularExpressions;

namespace IPNoticeHub.Infrastructure.Rendering.Implementation
{
    public class RegexTemplateTokenReplacer : ITemplateTokenReplacer
    {
        private static readonly Regex TokenRegex = 
            new(@"{{\s*(\w+)\s*}}", RegexOptions.Compiled);

        public string ReplaceTemplate(string template, IReadOnlyDictionary<string, string> tokens)
        {
            template ??= string.Empty;

            return TokenRegex.Replace(template, m =>
            {
                var key = m.Groups[1].Value;
                return tokens.TryGetValue(key, out var val) ? val : m.Value;
            });
        }
    }
}
