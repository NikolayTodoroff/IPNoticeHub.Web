namespace IPNoticeHub.Application.Rendering.Abstractions
{
    public interface ITemplateTokenReplacer
    {
        string ReplaceTemplate(string template, IReadOnlyDictionary<string, string> tokens);
    }
}
