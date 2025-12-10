using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Data.Entities.LegalDocuments;
using IPNoticeHub.Data.Repositories.DocumentLibrary.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace IPNoticeHub.Data.Repositories.DocumentLibrary.Implementations
{
    public class DocumentLibraryRepository : IDocumentLibraryRepository
    {
        private readonly IPNoticeHubDbContext dbContext;

        public DocumentLibraryRepository(IPNoticeHubDbContext context)
        {
            dbContext = context;
        }

        public async Task<int> AddAsync(LegalDocument document, CancellationToken cancellationToken = default)
        {
            if (document is null) throw new ArgumentNullException(nameof(document));
            if (document.CreatedOn == default) document.CreatedOn = DateTime.UtcNow;

            await dbContext.LegalDocuments.AddAsync(document, cancellationToken);
            await dbContext.SaveChangesAsync();

            return document.Id;
        }

        public async Task<LegalDocument?> GetDocumentByIdAsync(int documentId, string userId, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("UserId cannot be null or whitespace.", nameof(userId));
            }

            return await dbContext.LegalDocuments.
                AsNoTracking().
                FirstOrDefaultAsync(
                d => d.Id == documentId && 
                d.UserId == userId,
                cancellationToken);
        }
        public async Task<IReadOnlyList<LegalDocument>> GetUserDocumentsAsync(string userId, 
            DocumentSourceType? sourceType = null, LetterTemplateType? templateType = null, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("UserId cannot be null or whitespace.", nameof(userId));
            }

            IQueryable<LegalDocument> query = dbContext.LegalDocuments.
                AsNoTracking().
                Where(d => d.UserId == userId);

            if (sourceType.HasValue)
            {
                query = query.Where(d => d.SourceType == sourceType.Value);
            }

            if (templateType.HasValue)
            {
                query = query.Where(d => d.TemplateType == templateType.Value);
            }

            return await query.
                OrderByDescending(d => d.CreatedOn).
                ToListAsync(cancellationToken);
        }

        public async Task RenameAsync(int documentId, string userId, string newTitle, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("UserId cannot be null or whitespace.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(newTitle))
            {
                throw new ArgumentException("New title cannot be null or whitespace.", nameof(newTitle));
            }

            var document = await dbContext.LegalDocuments.
                FirstOrDefaultAsync(
                d => d.Id == documentId && d.UserId == userId,
                cancellationToken);

            if (document is null) return;

            document.DocumentTitle= newTitle;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task SoftDeleteAsync(int documentId, string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("UserId cannot be null or whitespace.", nameof(userId));
            }

            var document = await dbContext.LegalDocuments.
                FirstOrDefaultAsync(
                d => d.Id == documentId && d.UserId == userId,
                cancellationToken);

            if (document is null) return;

            document.IsDeleted = true;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
