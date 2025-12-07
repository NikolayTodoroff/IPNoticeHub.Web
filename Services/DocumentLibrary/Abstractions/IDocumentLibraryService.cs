using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.DocumentLibrary.DTOs;

namespace IPNoticeHub.Services.DocumentLibrary.Abstractions
{
    public interface IDocumentLibraryService
    {
        Task<int> SaveDocumentAsync(
            string userId, 
            DocumentCreateDto dto,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<DocumentListItemDto>>GetUserDocumentsAsync(
            string userId,
            DocumentSourceType? sourceType = null,
            LetterTemplateType? templateType = null,
            CancellationToken cancellationToken = default);

        Task<bool> RenameDocumentAsync(
            int documentId,
            string userId,
            string newTitle,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteDocumentAsync(
            int documentId,
            string userId,
            CancellationToken cancellationToken = default);

        Task<(string fileName, byte[] Pdf)?> RestoreDocumentSnapshotAsync(
            int documentId,
            string userId,
            CancellationToken cancellationToken = default);
    }
}
