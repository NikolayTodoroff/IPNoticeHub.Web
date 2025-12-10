using IPNoticeHub.Shared.Enums;

namespace IPNoticeHub.Services.PdfGeneration.Abstractions
{
    public interface ILetterTemplateProvider
    {
        IReadOnlyList<LetterTemplatePreset> GetLetterTemplatePresets(LetterTemplateType type);
        LetterTemplatePreset? GetTemplateByKey(string key);
    }
 
    public sealed record LetterTemplatePreset(
        LetterTemplateType Type,
        string Key,
        string DisplayName,
        string BodyTemplate
    );
}
