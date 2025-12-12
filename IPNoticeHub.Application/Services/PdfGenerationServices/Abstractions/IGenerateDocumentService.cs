using IPNoticeHub.Application.DTOs.PdfDTOs;

namespace IPNoticeHub.Application.Services.PdfGenerationServices.Abstractions
{
    public interface IGenerateDocumentService
    {
        Task<byte[]> GenerateFromInputAsync(
            LetterInputDto input, 
            CancellationToken ct = default);

        Task<byte[]> GenerateFromSnapshotAsync(
            Guid legalDocumentPublicId, 
            string userId, 
            CancellationToken ct = default);
    }
}
