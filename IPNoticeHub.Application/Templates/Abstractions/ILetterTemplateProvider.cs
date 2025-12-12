using IPNoticeHub.Application.DTOs.PdfDTOs;

namespace IPNoticeHub.Application.Templates.Abstractions
{
    public interface ILetterTemplateProvider
    {
        LetterTemplateDto? GetTemplateByKey(string letterKey);
    }
}
