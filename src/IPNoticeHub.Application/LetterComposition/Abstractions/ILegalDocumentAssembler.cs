using IPNoticeHub.Application.DTOs.PdfDTOs;
using IPNoticeHub.Domain.Entities.LegalDocuments;

namespace IPNoticeHub.Application.LetterComposition.Abstractions
{
    public interface ILegalDocumentAssembler
    {
        PdfLetterDto RebuildFromSavedDocument(LegalDocument doc);
    }
}
