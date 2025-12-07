using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Entities.LegalDocuments;

namespace IPNoticeHub.Data.Repositories.DocumentLibrary.Abstractions
{
    public interface ILegalDocumentRepository
    {
        Task<int> AddAsync(LegalDocument document,CancellationToken cancellationToken = default);

        Task<IReadOnlyList<LegalDocument>> GetUserDocumentsAsync(string userId, 
            DocumentSourceType? sourceType = null,LetterTemplateType? templateType = null, 
            CancellationToken cancellationToken = default);

        Task<LegalDocument?> GetDocumentByIdAsync(int documentId, string userId, 
            CancellationToken cancellationToken = default);

        Task RenameAsync(int documentId, string userId,string newTitle, 
            CancellationToken cancellationToken = default);

        Task SoftDeleteAsync(int documentId, string userId, CancellationToken cancellationToken = default);
    }
}
