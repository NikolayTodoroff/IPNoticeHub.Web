namespace IPNoticeHub.Web.Infrastructure
{
    public static class TemplateReplacer
    {
        public static string ReplaceTemplate(string template, IDictionary<string, string> vars)
        {
            return System.Text.RegularExpressions.Regex.Replace(template ?? string.Empty, "{{\\s*(\\w+)\\s*}}", m =>
            {
                var key = m.Groups[1].Value;
                return vars.TryGetValue(key, out var val) ? (val ?? string.Empty) : m.Value;
            });
        }
    }
}
