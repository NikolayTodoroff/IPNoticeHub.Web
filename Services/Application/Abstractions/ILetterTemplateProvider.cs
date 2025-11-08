namespace IPNoticeHub.Services.Application.Abstractions
{
    public interface ILetterTemplateProvider
    {
        IReadOnlyList<LetterTemplatePreset> GetLetterTemplatePresets(LetterTemplateType type);
        LetterTemplatePreset? GetTemplateByKey(string key);
    }

    public enum LetterTemplateType { CeaseDesist, Dmca }

    public sealed record LetterTemplatePreset(
        LetterTemplateType Type,
        string Key,               // e.g. "CND-Universal", "CND-Copyright", "CND-Trademark", "DMCA-General"
        string DisplayName,       // e.g. "Cease & Desist (Universal)"
        string BodyTemplate       // the actual text with {{placeholders}}
    );
}
