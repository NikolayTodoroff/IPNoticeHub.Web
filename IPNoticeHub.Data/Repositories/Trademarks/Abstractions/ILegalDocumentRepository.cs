using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Entities.LegalDocuments;

namespace IPNoticeHub.Data.Repositories.Trademarks.Abstractions
{
    public interface ILegalDocumentRepository
    {
        Task<int> AddAsync(LegalDocument document,CancellationToken cancellationToken = default);

        Task<IReadOnlyList<LegalDocument>> GetUserDocumentsAsync(string useerId, DocumentSourceType? sourceType = null,
            LetterTemplateType? templateType = null, CancellationToken cancellationToken = default);

        Task<LegalDocument> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

        Task RenameAsync(int id,string userId,string newTitle, CancellationToken cancellationToken = default);

        Task SoftDeleteAsync(int id, string userId, CancellationToken cancellationToken = default);
    }
}
