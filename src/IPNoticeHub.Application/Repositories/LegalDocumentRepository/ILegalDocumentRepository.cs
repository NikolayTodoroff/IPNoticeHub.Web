using IPNoticeHub.Domain.Entities.LegalDocuments;

namespace IPNoticeHub.Application.Repositories.LegalDocumentRepository
{
    public interface ILegalDocumentRepository
    {
        Task<LegalDocument?> GetByPublicIdAsync(
            Guid publicId, 
            string userId, 
            CancellationToken ct = default);
    }
}
