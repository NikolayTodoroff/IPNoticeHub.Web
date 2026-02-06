using IPNoticeHub.Application.DTOs.PdfDTOs;
using IPNoticeHub.Domain.Entities.LegalDocuments;

namespace IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions
{
    public interface IPdfLetterService
    {
        Task<byte[]> GenerateFromInputAsync(
        LetterInputDto input,
        CancellationToken cancellationToken = default);

        Task<byte[]> GenerateFromSavedDocumentAsync(
            LegalDocument document,
            CancellationToken cancellationToken = default);
    }
}
