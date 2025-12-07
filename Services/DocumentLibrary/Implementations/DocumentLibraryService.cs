using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data.Entities.LegalDocuments;
using IPNoticeHub.Data.Repositories.DocumentLibrary.Abstractions;
using IPNoticeHub.Services.DocumentLibrary.Abstractions;
using IPNoticeHub.Services.DocumentLibrary.DTOs;
using System.Globalization;
using static IPNoticeHub.Common.ValidationConstants.FormattingConstants;
namespace IPNoticeHub.Services.DocumentLibrary.Implementations
{
    public class DocumentLibraryService : IDocumentLibraryService
    {
        private readonly ILegalDocumentRepository documentRepository;

        public DocumentLibraryService(ILegalDocumentRepository repository)
        {
            documentRepository = repository;
        }

        public async Task<int> SaveDocumentAsync(
            string userId, 
            DocumentCreateDto dto, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "UserId cannot be null or whitespace.", nameof(userId));
            } 

            if (dto is null) throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.BodyTemplate))
            {
                throw new ArgumentException(
                    "BodyTemplate cannot be null or whitespace.",
                    nameof(dto.BodyTemplate));
            }
            
            string title = string.IsNullOrWhiteSpace(dto.DocumentTitle) ? 
                GenerateDefaultTitle(dto) : dto.DocumentTitle;

            var entity = new LegalDocument
            {
                UserId = userId,
                RelatedPublicId = dto.RelatedPublicId,
                SourceType = dto.SourceType,
                TemplateType = dto.TemplateType,
                DocumentTitle = title,
                IpTitle = dto.IpTitle,
                RegistrationNumber = dto.RegistrationNumber,
                BodyTemplate = dto.BodyTemplate,
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false
            };

            return await documentRepository.AddAsync(entity, cancellationToken);
        }

        public async Task<IReadOnlyList<DocumentListItemDto>> GetUserDocumentsAsync(
            string userId, 
            DocumentSourceType? sourceType = null, 
            LetterTemplateType? templateType = null, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(
                    "UserId cannot be null or whitespace.",nameof(userId));

            var documents = 
                await documentRepository.GetUserDocumentsAsync(
                userId,
                sourceType,
                templateType,
                cancellationToken);

            return documents.
                Select(d => new DocumentListItemDto
            {
                    Id = d.Id,
                    DocumentTitle = d.DocumentTitle,
                    SourceType = d.SourceType,
                    TemplateType = d.TemplateType,
                    IpTitle = d.IpTitle,
                    RegistrationNumber = d.RegistrationNumber,
                    CreatedOn = d.CreatedOn
                })
                .ToList();
        }

        public async Task<bool>RenameDocumentAsync(
            int documentId, 
            string userId, 
            string newTitle, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(
                    "UserId cannot be null or whitespace.", nameof(userId));

            if (string.IsNullOrWhiteSpace(newTitle))
                throw new ArgumentException(
                    "New title cannot be null or whitespace.", nameof(newTitle));

            await documentRepository.RenameAsync(
                documentId,
                userId, 
                newTitle, 
                cancellationToken);

            return true;
        }

        public async Task<bool>DeleteDocumentAsync(
            int documentId, 
            string userId, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(
                    "UserId cannot be null or whitespace.", nameof(userId));

            await documentRepository.SoftDeleteAsync(
                documentId, 
                userId, 
                cancellationToken);

            return true;
        }  

        private static string GenerateDefaultTitle(DocumentCreateDto dto)
        {
            string ipType = dto.TemplateType == LetterTemplateType.CeaseAndDesist ?
                "Cease & Desist" : "DMCA Notice";

            string ipTitle = string.IsNullOrWhiteSpace(dto.IpTitle) ? 
                string.Empty : $" – {dto.IpTitle}";

            string date = DateTime.UtcNow.ToString(
                DateTimeFormat,CultureInfo.InvariantCulture);

            return $"{ipType}{ipTitle} – {date}";
        }
    }
}
