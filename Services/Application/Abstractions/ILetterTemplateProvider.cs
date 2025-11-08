using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Services.Application.Abstractions
{
    public interface ILetterTemplateProvider
    {
        IReadOnlyList<LetterTemplatePreset> GetLetterTemplatePresets(LetterTemplateType type);
        LetterTemplatePreset? GetTemplateByKey(string key);
    }
 
    public sealed record LetterTemplatePreset(
        LetterTemplateType Type,
        string Key,               // e.g. "CND-Copyright", "CND-Trademark", "DMCA-General"
        string DisplayName,       // e.g. "Cease & Desist (Universal)"
        string BodyTemplate       // the template text with {{placeholders}}
    );
}
